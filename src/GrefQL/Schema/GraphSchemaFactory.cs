using System.Diagnostics;
using System.Reflection;
using GraphQL.Types;
using GrefQL.Query;
using GrefQL.Types;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GrefQL.Schema
{
    public class GraphSchemaFactory : IGraphSchemaFactory
    {
        private readonly IGraphTypeMapper _typeMapper;
        private readonly IFieldResolverFactory _resolveFactory;
        private readonly IGraphTypeResolverSource _graphTypeResolverSource;

        public GraphSchemaFactory(IGraphTypeMapper typeMapper,
            IFieldResolverFactory resolveFactory,
            IGraphTypeResolverSource graphTypeResolverSource)
        {
            _typeMapper = typeMapper;
            _resolveFactory = resolveFactory;
            _graphTypeResolverSource = graphTypeResolverSource;
        }

        public ISchema Create(IModel model)
        {
            var schema = new GraphQL.Types.Schema(_graphTypeResolverSource.Resolve);
            var query = schema.Query = new ObjectGraphType();
            foreach (var entityType in model.GetEntityTypes())
            {
                var boundMethod = _AddEntityType.MakeGenericMethod(entityType.ClrType);
                boundMethod.Invoke(this, new object[] { query, entityType });
            }

            return schema;
        }

        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo _AddEntityType
            = typeof(GraphSchemaFactory)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(AddEntityType));

        private void AddEntityType<TEntity>(ObjectGraphType schema, IEntityType entityType)
        {
            CreateGraphType<TEntity>(entityType);
            
            // TODO ensure this is in a safe format for the schema
            var fieldName = entityType.DisplayName().Camelize();
            var description = entityType.GraphQL().DescriptionOrDefault();
            schema.AddField<ObjectGraphType<TEntity>>(fieldName, description, _resolveFactory.CreateResolveEntityByKey(entityType));

            var listFieldName = fieldName.Pluralize();
            Debug.Assert(fieldName != listFieldName, "pluralized version cannot match singular version");
            schema.AddField<ListGraphType<ObjectGraphType<TEntity>>>(listFieldName, description, _resolveFactory.CreateResolveEntityList(entityType));
        }

        private void CreateGraphType<TEntity>(IEntityType entityType)
        {
            var graphType = new ObjectGraphType<TEntity>();

            foreach (var prop in entityType.GetProperties())
            {
                var fieldGraphType = _typeMapper.FindMapping(prop);
                if (fieldGraphType == null)
                {
                    // TODO handle unmapped clr types
                    continue;
                }
                graphType.AddField(fieldGraphType, prop.GraphQL().NameOrDefault(), prop.GraphQL().DescriptionOrDefault());
            }

            _graphTypeResolverSource.AddResolver<ObjectGraphType<TEntity>>(() => graphType);
        }
    }
}

using System.Diagnostics;
using System.Reflection;
using GraphQL.Types;
using GrefQL.Query;
using GrefQL.Types;
using Microsoft.EntityFrameworkCore.Metadata;

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

        private void AddEntityType<TEntity>(GraphType query, IEntityType entityType)
        {
            CreateGraphType<TEntity>(entityType);

            // TODO ensure this is in a safe format for the schema
            var fieldName = entityType.GraphQL().FieldName;

            query.AddField<ObjectGraphType<TEntity>>(
                fieldName, 
                entityType.GraphQL().Description,
                _resolveFactory.CreateResolveEntityByKey(entityType));

            var listFieldName = entityType.GraphQL().PluralFieldName;

            Debug.Assert(fieldName != listFieldName, "pluralized version cannot match singular version");

            query.AddField<ListGraphType<ObjectGraphType<TEntity>>>(
                listFieldName, 
                entityType.GraphQL().PluralDescription, 
                _resolveFactory.CreateResolveEntityList(entityType));
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
                graphType.AddField(fieldGraphType, prop.GraphQL().FieldName, prop.GraphQL().Description);
            }

            _graphTypeResolverSource.AddResolver<ObjectGraphType<TEntity>>(() => graphType);
        }
    }
}

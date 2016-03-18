using System.Linq;
using System.Reflection;
using GraphQL.Types;
using GrefQL.Query;
using GrefQL.Types;
using Microsoft.EntityFrameworkCore;
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
                var fieldType 
                    = AddEntityCollectionField(
                        query,
                        entityType,
                        _resolveFactory.CreateResolveEntityList(entityType));

                var entityCountFieldResolver = _resolveFactory.CreateResolveEntityCount(entityType);

                query.Field<IntGraphType>(
                    $"{fieldType.Name}Count",
                    $"Gets the total number of {fieldType.Name}.",
                    arguments: entityCountFieldResolver.Arguments,
                    resolve: entityCountFieldResolver.Resolve);
            }

            return schema;
        }

        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo _addEntityField
            = typeof(GraphSchemaFactory)
                .GetTypeInfo()
                .GetDeclaredMethods(nameof(AddEntityField))
                .Single(m => m.ContainsGenericParameters);

        private void AddEntityField(GraphType query, IEntityType entityType, FieldResolver resolver)
            => _addEntityField
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(this, new object[] { query, entityType, resolver });

        private void AddEntityField<TEntity>(GraphType query, IEntityType entityType, FieldResolver resolver)
            where TEntity : class
        {
            CreateEntityGraphType<TEntity>(entityType);

            // TODO use a different resolver when this is a nav prop
            query.AddField<ObjectGraphType<TEntity>>(
                entityType.GraphQL().FieldName,
                entityType.GraphQL().Description,
                resolver);
        }

        private static readonly MethodInfo _addEntityCollectionFieldMethod
            = typeof(GraphSchemaFactory)
                .GetTypeInfo()
                .GetDeclaredMethods(nameof(AddEntityCollectionField))
                .Single(m => m.ContainsGenericParameters);

        private FieldType AddEntityCollectionField(GraphType query, IEntityType entityType, FieldResolver resolver)
            => (FieldType)_addEntityCollectionFieldMethod
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(this, new object[] { query, entityType, resolver });

        private FieldType AddEntityCollectionField<TEntity>(GraphType query, IEntityType entityType, FieldResolver resolver)
            where TEntity : class
        {
            CreateEntityGraphType<TEntity>(entityType);

            var listFieldName = entityType.GraphQL().PluralFieldName;

            return query.AddField<ListGraphType<ObjectGraphType<TEntity>>>(
                listFieldName,
                entityType.GraphQL().PluralDescription,
                resolver);
        }

        private void CreateEntityGraphType<TEntity>(IEntityType entityType)
            where TEntity : class
        {
            ObjectGraphType<TEntity> graphType;
            if (_graphTypeResolverSource.TryResolve(out graphType))
            {
                return;
            }

            graphType = new ObjectGraphType<TEntity>();

            _graphTypeResolverSource.AddResolver(() => graphType);

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

            foreach (var navigation in entityType.GetNavigations())
            {
                AddNavigation(navigation, graphType);
            }
        }

        private void AddNavigation(INavigation navigation, GraphType declaringType)
        {
            var target = navigation.GetTargetType();

            if (navigation.IsCollection())
            {
                AddEntityCollectionField(
                    declaringType, target, _resolveFactory.CreateResolveNavigation(navigation));
            }
            else
            {
                AddEntityField(
                    declaringType, target, _resolveFactory.CreateResolveNavigation(navigation));
            }
        }
    }
}

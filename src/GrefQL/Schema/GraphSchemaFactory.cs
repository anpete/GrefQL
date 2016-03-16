using System;
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
                AddEntityType(entityType.ClrType, query, entityType);
                AddEntityTypeCollection(entityType.ClrType, query, entityType);
            }

            return schema;
        }

        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo _AddEntityType
            = typeof (GraphSchemaFactory)
                .GetTypeInfo()
                .GetDeclaredMethods(nameof(AddEntityType))
                .Single(m => m.ContainsGenericParameters);

        private void AddEntityType(Type clrType, GraphType query, IEntityType entityType)
        {
            var boundMethod = _AddEntityType.MakeGenericMethod(clrType);
            boundMethod.Invoke(this, new object[] { query, entityType });
        }

        private void AddEntityType<TEntity>(GraphType query, IEntityType entityType)
            where TEntity : class
        {
            CreateGraphType<TEntity>(entityType);

            // TODO use a different resolver when this is a nav prop
            query.AddField<ObjectGraphType<TEntity>>(
                entityType.GraphQL().FieldName,
                entityType.GraphQL().Description,
                _resolveFactory.CreateResolveEntityByKey(entityType));
        }

        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo _AddEntityTypeCollection
            = typeof (GraphSchemaFactory)
                .GetTypeInfo()
                .GetDeclaredMethods(nameof(AddEntityTypeCollection))
                .Single(m => m.ContainsGenericParameters);

        private void AddEntityTypeCollection(Type clrType, GraphType query, IEntityType entityType)
        {
            var boundMethod = _AddEntityTypeCollection.MakeGenericMethod(clrType);
            boundMethod.Invoke(this, new object[] { query, entityType });
        }

        private void AddEntityTypeCollection<TEntity>(GraphType query, IEntityType entityType)
            where TEntity : class
        {
            CreateGraphType<TEntity>(entityType);

            var listFieldName = entityType.GraphQL().PluralFieldName;

            query.AddField<ListGraphType<ObjectGraphType<TEntity>>>(
                listFieldName,
                entityType.GraphQL().PluralDescription,
                _resolveFactory.CreateResolveEntityList(entityType));
        }

        private GraphType CreateGraphType<TEntity>(IEntityType entityType)
            where TEntity : class
        {
            ObjectGraphType<TEntity> graphType;
            if (_graphTypeResolverSource.TryResolve(out graphType))
            {
                return graphType;
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

            foreach (var navProp in entityType.GetNavigations())
            {
                var principalEntityType = navProp.GetTargetType();
                if (navProp.IsCollection())
                {
                    AddEntityTypeCollection(principalEntityType.ClrType, graphType, principalEntityType);
                }
                else
                {
                    AddEntityType(principalEntityType.ClrType, graphType, principalEntityType);
                }
            }

            return graphType;
        }
    }
}

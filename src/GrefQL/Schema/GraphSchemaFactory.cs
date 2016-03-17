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
                AddEntityCollectionField(entityType.ClrType, query, entityType, _resolveFactory.CreateResolveEntityList(entityType));
            }

            return schema;
        }

        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo _AddEntityType
            = typeof (GraphSchemaFactory)
                .GetTypeInfo()
                .GetDeclaredMethods(nameof(AddEntityField))
                .Single(m => m.ContainsGenericParameters);

        private void AddEntityField(Type clrType, GraphType query, IEntityType entityType, FieldResolver resolver)
        {
            var boundMethod = _AddEntityType.MakeGenericMethod(clrType);
            boundMethod.Invoke(this, new object[] { query, entityType, resolver });
        }

        private void AddEntityField<TEntity>(GraphType query, IEntityType entityType, FieldResolver resolver)
            where TEntity : class
        {
            CreateGraphType<TEntity>(entityType);

            // TODO use a different resolver when this is a nav prop
            query.AddField<ObjectGraphType<TEntity>>(
                entityType.GraphQL().FieldName,
                entityType.GraphQL().Description,
                resolver);
        }

        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo _AddEntityTypeCollection
            = typeof (GraphSchemaFactory)
                .GetTypeInfo()
                .GetDeclaredMethods(nameof(AddEntityCollectionField))
                .Single(m => m.ContainsGenericParameters);

        private void AddEntityCollectionField(Type clrType, GraphType query, IEntityType entityType, FieldResolver resolver)
        {
            var boundMethod = _AddEntityTypeCollection.MakeGenericMethod(clrType);
            boundMethod.Invoke(this, new object[] { query, entityType, resolver });
        }

        private void AddEntityCollectionField<TEntity>(GraphType query, IEntityType entityType, FieldResolver resolver)
            where TEntity : class
        {
            CreateGraphType<TEntity>(entityType);

            var listFieldName = entityType.GraphQL().PluralFieldName;

            query.AddField<ListGraphType<ObjectGraphType<TEntity>>>(
                listFieldName,
                entityType.GraphQL().PluralDescription,
                resolver);
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

            foreach (var nav in entityType.GetNavigations())
            {
                AddNavigation(nav, graphType);
            }

            return graphType;
        }

        private void AddNavigation(INavigation navigation, GraphType declaringType)
        {
            var target = navigation.GetTargetType();
            if (navigation.IsCollection())
            {
                AddEntityCollectionField(target.ClrType, declaringType, target, _resolveFactory.CreateResolveNavigation(navigation));
            }
            else
            {
                AddEntityField(target.ClrType, declaringType, target, _resolveFactory.CreateResolveNavigation(navigation));
            }
        }
    }
}

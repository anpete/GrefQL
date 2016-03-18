using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using GraphQL.Types;
using GrefQL.Schema;
using GrefQL.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable LoopCanBeConvertedToQuery

// ReSharper disable AccessToModifiedClosure

namespace GrefQL.Query
{
    public class FieldResolverFactory : IFieldResolverFactory
    {
        private const string OffsetArgumentName = "offset";
        private const string LimitArgumentName = "limit";
        private const string OrderByArgumentName = "orderBy";
        private const string FieldFieldName = "field";
        private const string DirectionFieldName = "direction";
        private const string AscendingEnumName = "ASC";
        private const string DescendingEnumName = "DESC";

        public FieldResolverFactory(IGraphTypeMapper typeMapper)
        {
            _typeMapper = typeMapper;
        }

        private readonly IGraphTypeMapper _typeMapper;

        private static readonly ParameterExpression _resolveFieldContextParameterExpression
            = Expression.Parameter(typeof(ResolveFieldContext), "resolveFieldContext");

        private static readonly MethodInfo _efPropertyMethodInfo
            = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(Property));

        private static readonly List<QueryArgument> _listArguments = new List<QueryArgument>
        {
            new QueryArgument<IntGraphType>
            {
                Name = OffsetArgumentName,
                Description = "The number of results to skip."
            },
            new QueryArgument<IntGraphType>
            {
                Name = LimitArgumentName,
                Description = "The maximum number of results to return."
            },
            new QueryArgument<ListGraphType<OrderingType>>
            {
                Name = OrderByArgumentName,
                Description = "An option list of Orderings used to order the query results."
            }
        };

        public class OrderingDirectionType : EnumerationGraphType
        {
            public OrderingDirectionType()
            {
                Name = "OrderingDirection";
                Description = $"Specifies the ordering direction: ${AscendingEnumName} or ${DescendingEnumName}.";

                AddValue(AscendingEnumName, "The ascending direction.", AscendingEnumName);
                AddValue(DescendingEnumName, "The descending direction.", DescendingEnumName);
            }
        }

        public class OrderingType : InputObjectGraphType
        {
            public OrderingType()
            {
                Name = "Ordering";
                Description = "Specifies an ordering: A field to order on, and an optional ordering direction.";

                // TODO: Generate an enum for the field names
                Field<NonNullGraphType<StringGraphType>>(FieldFieldName, "The field to order on.");
                Field<OrderingDirectionType>(DirectionFieldName, "The optional ordering direction.");
            }
        }

        public FieldResolver CreateResolveEntityCount(IEntityType entityType)
        {
            var countAsyncCallExpression
                = Expression.Call(
                    _countAsyncMethodInfo.MakeGenericMethod(entityType.ClrType),
                    _resolveFieldContextParameterExpression,
                    Expression.Constant(entityType));

            var resolveLambdaExpression
                = Expression
                    .Lambda<Func<ResolveFieldContext, object>>(
                        countAsyncCallExpression,
                        _resolveFieldContextParameterExpression);

            var queryArguments = new List<QueryArgument>();

            AddFilterQueryArguments(entityType, queryArguments);

            return new FieldResolver
            {
                Resolve = resolveLambdaExpression.Compile(),
                Arguments = new QueryArguments(queryArguments)
            };
        }

        private static readonly MethodInfo _countAsyncMethodInfo
            = typeof(FieldResolverFactory).GetTypeInfo().GetDeclaredMethod(nameof(CountAsync));

        private static async Task<int> CountAsync<TEntity>(
            ResolveFieldContext resolveFieldContext, IEntityType entityType)
            where TEntity : class
        {
            var queryExecutionContext = (QueryExecutionContext)resolveFieldContext.RootValue;
            var contextSemaphore = queryExecutionContext.ContextSemaphore;

            await contextSemaphore.WaitAsync(resolveFieldContext.CancellationToken);

            try
            {
                if (queryExecutionContext.DbContext == null)
                {
                    throw new InvalidOperationException("Cannot find dbContext in current field context");
                }

                IQueryable<TEntity> query = queryExecutionContext.DbContext.Set<TEntity>();

                query = ApplyFilters(resolveFieldContext, query, entityType);
                
                return await query.CountAsync();
            }
            finally
            {
                contextSemaphore.Release();
            }
        }

        public FieldResolver CreateResolveEntityList(IEntityType entityType)
        {
            var queryEntitiesAsyncCallExpression
                = Expression.Call(
                    _toArrayAsyncMethodInfo.MakeGenericMethod(entityType.ClrType),
                    _resolveFieldContextParameterExpression,
                    Expression.Constant(entityType));

            var resolveLambdaExpression
                = Expression
                    .Lambda<Func<ResolveFieldContext, object>>(
                        queryEntitiesAsyncCallExpression,
                        _resolveFieldContextParameterExpression);

            var queryArguments = _listArguments.ToList();

            AddFilterQueryArguments(entityType, queryArguments);

            return new FieldResolver
            {
                Resolve = resolveLambdaExpression.Compile(),
                Arguments = new QueryArguments(queryArguments)
            };
        }

        private void AddFilterQueryArguments(IEntityType entityType, ICollection<QueryArgument> queryArguments)
        {
            foreach (var property in GetFilterableProperties(entityType))
            {
                queryArguments.Add(
                    new QueryArgument(_typeMapper.FindMapping(property, notNull: true))
                    {
                        Name = property.GraphQL().FieldName,
                        Description = property.GraphQL().Description
                    });
            }
        }

        private static readonly MethodInfo _toArrayAsyncMethodInfo
            = typeof(FieldResolverFactory).GetTypeInfo().GetDeclaredMethod(nameof(ToArrayAsync));

        private static async Task<TEntity[]> ToArrayAsync<TEntity>(
            ResolveFieldContext resolveFieldContext, IEntityType entityType)
            where TEntity : class
        {
            var contextSemaphore = ((QueryExecutionContext)resolveFieldContext.RootValue).ContextSemaphore;

            await contextSemaphore.WaitAsync(resolveFieldContext.CancellationToken);

            try
            {
                return await Query<TEntity>(resolveFieldContext, entityType)
                    .ToArrayAsync(resolveFieldContext.CancellationToken);
            }
            finally
            {
                contextSemaphore.Release();
            }
        }

        private static readonly MethodInfo _firstAsyncMethodInfo
            = typeof(FieldResolverFactory).GetTypeInfo().GetDeclaredMethod(nameof(FirstAsync));

        private static async Task<TEntity> FirstAsync<TEntity>(
            ResolveFieldContext resolveFieldContext, IEntityType entityType)
            where TEntity : class
        {
            var contextSemaphore = ((QueryExecutionContext)resolveFieldContext.RootValue).ContextSemaphore;

            await contextSemaphore.WaitAsync(resolveFieldContext.CancellationToken);

            try
            {
                return await Query<TEntity>(resolveFieldContext, entityType)
                    .FirstAsync(resolveFieldContext.CancellationToken);
            }
            finally
            {
                contextSemaphore.Release();
            }
        }

        private static IQueryable<TEntity> Query<TEntity>(ResolveFieldContext resolveFieldContext, IEntityType entityType)
            where TEntity : class
        {
            var exectionContext = resolveFieldContext.RootValue as QueryExecutionContext;

            if (exectionContext?.DbContext == null)
            {
                throw new InvalidOperationException("Cannot find dbContext in current field context");
            }

            var query = exectionContext.DbContext.Set<TEntity>().AsNoTracking();

            // TODO: Probably need a GraphQL->LINQ query cache here.

            query = ApplyFilters(resolveFieldContext, query, entityType);
            query = ApplyOrderBys(resolveFieldContext, query, entityType);
            query = ApplyProjection(resolveFieldContext, query, entityType);

            TryApplyArgument<int>(resolveFieldContext, OffsetArgumentName, offset => query = query.Skip(offset));
            TryApplyArgument<int>(resolveFieldContext, LimitArgumentName, limit => query = query.Take(limit));

            return query;
        }

        private static IQueryable<TEntity> ApplyFilters<TEntity>(
            ResolveFieldContext resolveFieldContext,
            IQueryable<TEntity> query,
            IEntityType entityType)
            where TEntity : class
        {
            var entityParameterExpression = Expression.Parameter(typeof(TEntity), "entity");

            Expression predicateExpression = null;

            // TODO: Consider deriving QueryArgument so we don't need entityType here.
            //       Also consider stashing entityType on the top-level field
            //       Predicate eval order should probably match argument order
            foreach (var property in GetFilterableProperties(entityType))
            {
                TryApplyArgument<object>(resolveFieldContext, property.GraphQL().FieldName, value =>
                    {
                        var equalExpression
                            = Expression.Equal(
                                Expression.Call(
                                    null,
                                    _efPropertyMethodInfo.MakeGenericMethod(property.ClrType),
                                    entityParameterExpression,
                                    Expression.Constant(property.Name)),
                                Expression.Constant(value));

                        predicateExpression
                            = predicateExpression == null
                                ? equalExpression
                                : Expression.AndAlso(predicateExpression, equalExpression);
                    });
            }

            if (predicateExpression != null)
            {
                var predicateLambda
                    = Expression.Lambda<Func<TEntity, bool>>(
                        predicateExpression,
                        entityParameterExpression);

                query = query.Where(predicateLambda);
            }

            return query;
        }

        private static IEnumerable<IProperty> GetFilterableProperties(IEntityType entityType)
        {
            return entityType.GetKeys().SelectMany(i => i.Properties)
                .Union(entityType.GetForeignKeys().SelectMany(i => i.Properties))
                .Union(entityType.GetIndexes().SelectMany(i => i.Properties));
        }

        private static IQueryable<TEntity> ApplyOrderBys<TEntity>(
            ResolveFieldContext resolveFieldContext,
            IQueryable<TEntity> query,
            IEntityType entityType)
            where TEntity : class
        {
            TryApplyArgument<object[]>(resolveFieldContext, OrderByArgumentName, orderBy =>
                {
                    var firstOrdering = true;
                    var entityParameterExpression = Expression.Parameter(typeof(TEntity), "entity");

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (Dictionary<string, object> ordering in orderBy)
                    {
                        var field = ((string)ordering[FieldFieldName]).ToPascalCase();
                        var property = entityType.FindProperty(field);

                        // TODO: Error case.

                        object direction;
                        var descending
                            = ordering.TryGetValue(DirectionFieldName, out direction)
                              && (string)direction == DescendingEnumName;

                        var propertyExpression
                            = Expression.Call(
                                null,
                                _efPropertyMethodInfo.MakeGenericMethod(property.ClrType),
                                entityParameterExpression,
                                Expression.Constant(property.Name));

                        query = (IQueryable<TEntity>)
                            _applyOrderByMethodInfo
                                .MakeGenericMethod(typeof(TEntity), propertyExpression.Type)
                                .Invoke(null, new object[]
                                {
                                    query,
                                    entityParameterExpression,
                                    propertyExpression,
                                    descending,
                                    firstOrdering
                                });

                        firstOrdering = false;
                    }
                });

            return query;
        }

        public static MethodInfo _applyOrderByMethodInfo
            = typeof(FieldResolverFactory).GetTypeInfo().GetDeclaredMethod(nameof(ApplyOrderBy));

        private static IQueryable<TEntity> ApplyOrderBy<TEntity, TKey>(
            IOrderedQueryable<TEntity> query,
            ParameterExpression entityParameterExpression,
            Expression propertyExpression,
            bool descending,
            bool firstOrdering)
        {
            var keySelector
                = Expression.Lambda<Func<TEntity, TKey>>(
                    propertyExpression,
                    entityParameterExpression);

            if (firstOrdering)
            {
                return !descending
                    ? query.OrderBy(keySelector)
                    : query.OrderByDescending(keySelector);
            }

            return !descending
                ? query.ThenBy(keySelector)
                : query.ThenByDescending(keySelector);
        }

        private static IQueryable<TEntity> ApplyProjection<TEntity>(
            ResolveFieldContext resolveFieldContext, IQueryable<TEntity> query, IEntityType entityType)
        {
            var selectionSet = resolveFieldContext.FieldAst?.Selections;

            if (selectionSet == null)
            {
                return query;
            }

            var entityClrType = typeof(TEntity);
            var entityTypeInfo = entityClrType.GetTypeInfo();
            var entityParameterExpression = Expression.Parameter(entityClrType, "entity");
            var memberAssignments = new List<MemberAssignment>();

            foreach (var property 
                in selectionSet
                    .Select(s => entityType.FindProperty(s.Field.Name.ToPascalCase()))
                    .Where(p => p != null)
                    .Union(entityType.GetKeys().SelectMany(k => k.Properties))
                    .Union(entityType.GetForeignKeys().SelectMany(fk => fk.Properties)))
            {
                memberAssignments.Add(
                    Expression.Bind(
                        entityTypeInfo.GetProperty(property.Name),
                        Expression.Call(
                            null,
                            _efPropertyMethodInfo.MakeGenericMethod(property.ClrType),
                            entityParameterExpression,
                            Expression.Constant(property.Name))));
            }

            var selector
                = Expression.Lambda<Func<TEntity, TEntity>>(
                    Expression.MemberInit(
                        Expression.New(entityClrType),
                        memberAssignments),
                    entityParameterExpression);

            return query.Select(selector);
        }

        private static void TryApplyArgument<TArgument>(
            ResolveFieldContext resolveFieldContext,
            string argument,
            Action<TArgument> action)
        {
            object value = null;
            if (resolveFieldContext.Arguments?.TryGetValue(argument, out value) == true
                && value is TArgument)
            {
                action((TArgument)value);
            }
        }

        private static readonly MethodInfo _tryAddArgumentFromSource
            = typeof(FieldResolverFactory).GetTypeInfo().GetDeclaredMethod(nameof(TryAddArgumentFromSource));

        private static void TryAddArgumentFromSource(
            ResolveFieldContext resolveFieldContext,
            string argument,
            IClrPropertyGetter getter)
        {
            if (resolveFieldContext.Arguments == null)
            {
                resolveFieldContext.Arguments = new Dictionary<string, object>();
            }

            var value = getter.GetClrValue(resolveFieldContext.Source);

            Debug.Assert(value != null);

            resolveFieldContext.Arguments[argument] = value;
        }

        public FieldResolver CreateResolveNavigation(INavigation navigation)
        {
            var sourceProps = navigation.ForeignKey.Properties;
            var targetProps = navigation.ForeignKey.PrincipalKey.Properties;

            if (!navigation.IsDependentToPrincipal())
            {
                var tmp = targetProps;
                targetProps = sourceProps;
                sourceProps = tmp;
            }

            var blockExpressions = new List<Expression>();

            for (var i = 0; i < sourceProps.Count; i++)
            {
                var sourceProp = sourceProps[i];
                var targetProp = targetProps[i];

                var assignArgumentExpression
                    = Expression.Call(_tryAddArgumentFromSource,
                        _resolveFieldContextParameterExpression,
                        Expression.Constant(targetProp.GraphQL().FieldName),
                        Expression.Constant(sourceProp.GetGetter()));

                blockExpressions.Add(assignArgumentExpression);
            }

            var method = navigation.IsCollection()
                ? _toArrayAsyncMethodInfo
                : _firstAsyncMethodInfo;

            Expression queryEntitiesCallExpression
                = Expression.Call(
                    method.MakeGenericMethod(navigation.GetTargetType().ClrType),
                    _resolveFieldContextParameterExpression,
                    Expression.Constant(navigation.GetTargetType()));

            blockExpressions.Add(queryEntitiesCallExpression);

            var blockExpression = Expression.Block(blockExpressions);

            var resolveLambdaExpression = Expression
                .Lambda<Func<ResolveFieldContext, object>>(
                    blockExpression,
                    _resolveFieldContextParameterExpression);

            return new FieldResolver
            {
                Resolve = resolveLambdaExpression.Compile()
            };
        }
    }
}

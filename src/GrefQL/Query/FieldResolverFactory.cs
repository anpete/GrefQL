using System;
using System.Collections.Generic;
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

        public FieldResolver CreateResolveEntityList(IEntityType entityType)
        {
            var queryEntitiesAsyncCallExpression
                = Expression.Call(
                    _queryEntitiesAsyncMethodInfo.MakeGenericMethod(entityType.ClrType),
                    _resolveFieldContextParameterExpression,
                    Expression.Constant(entityType));

            var resolveLambdaExpression
                = Expression
                    .Lambda<Func<ResolveFieldContext, object>>(
                        queryEntitiesAsyncCallExpression,
                        _resolveFieldContextParameterExpression);

            var queryArguments = _listArguments.ToList();

            foreach (var property in GetFilterableProperties(entityType))
            {
                queryArguments.Add(
                    new QueryArgument(_typeMapper.FindMapping(property))
                    {
                        Name = property.GraphQL().FieldName,
                        Description = property.GraphQL().Description
                    });
            }

            return new FieldResolver
            {
                Resolve = resolveLambdaExpression.Compile(),
                Arguments = new QueryArguments(queryArguments)
            };
        }

        public static MethodInfo _queryEntitiesAsyncMethodInfo
            = typeof(FieldResolverFactory).GetTypeInfo().GetDeclaredMethod(nameof(QueryEntitiesAsync));

        private static Task<TEntity[]> QueryEntitiesAsync<TEntity>(
            ResolveFieldContext resolveFieldContext, IEntityType entityType)
            where TEntity : class
        {
            var dbContext = resolveFieldContext.Source as DbContext;

            if (dbContext == null)
            {
                return null;
            }

            var query = dbContext.Set<TEntity>().AsNoTracking();

            // TODO: Probably need a GraphQL->LINQ query cache here.

            TryApplyArgument<int>(resolveFieldContext, OffsetArgumentName, offset => query = query.Skip(offset));
            TryApplyArgument<int>(resolveFieldContext, LimitArgumentName, limit => query = query.Take(limit));

            query = TryApplyFilters(resolveFieldContext, query, entityType);
            query = TryApplyOrderBy(resolveFieldContext, query, entityType);

            return query.ToArrayAsync(resolveFieldContext.CancellationToken);
        }

        private static IQueryable<TEntity> TryApplyFilters<TEntity>(
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
                .Concat(entityType.GetForeignKeys().SelectMany(i => i.Properties))
                .Concat(entityType.GetIndexes().SelectMany(i => i.Properties));
        }

        private static IQueryable<TEntity> TryApplyOrderBy<TEntity>(
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
    }
}

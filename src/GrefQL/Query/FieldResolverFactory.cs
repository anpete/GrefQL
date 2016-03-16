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

                Field<NonNullGraphType<StringGraphType>>(FieldFieldName, "The field to order on.");
                Field<OrderingDirectionType>(DirectionFieldName, "The optional ordering direction.");
            }
        }

        public FieldResolver CreateResolveEntityList(IEntityType entityType)
        {
            var queryEntitiesAsyncCallExpression
                = Expression.Call(
                    _queryEntitiesAsyncMethodInfo.MakeGenericMethod(entityType.ClrType),
                    _resolveFieldContextParameterExpression);

            var resolveLambdaExpression
                = Expression
                    .Lambda<Func<ResolveFieldContext, object>>(
                        queryEntitiesAsyncCallExpression,
                        _resolveFieldContextParameterExpression);

            return new FieldResolver
            {
                Resolve = resolveLambdaExpression.Compile(),
                Arguments = new QueryArguments(_listArguments)
            };
        }

        public static MethodInfo _queryEntitiesAsyncMethodInfo
            = typeof(FieldResolverFactory).GetTypeInfo().GetDeclaredMethod(nameof(QueryEntitiesAsync));

        private static Task<TEntity[]> QueryEntitiesAsync<TEntity>(ResolveFieldContext resolveFieldContext)
            where TEntity : class
        {
            var dbContext = resolveFieldContext.Source as DbContext;

            if (dbContext == null)
            {
                return null;
            }

            IQueryable<TEntity> query = dbContext.Set<TEntity>();

            // TODO: Probably need a GraphQL->LINQ query cache here.

            TryApplyArgument<int>(resolveFieldContext, OffsetArgumentName, offset => query = query.Skip(offset));
            TryApplyArgument<int>(resolveFieldContext, LimitArgumentName, limit => query = query.Take(limit));

            query = TryApplyOrderBy(resolveFieldContext, query);

            return query.ToArrayAsync(resolveFieldContext.CancellationToken);
        }

        private static IQueryable<TEntity> TryApplyOrderBy<TEntity>(
            ResolveFieldContext resolveFieldContext, IQueryable<TEntity> query)
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
                        var propertyInfo = typeof(TEntity).GetTypeInfo().GetProperty(field);

                        var propertyExpression
                            = Expression.MakeMemberAccess(
                                entityParameterExpression,
                                propertyInfo);

                        object direction;
                        var descending
                            = ordering.TryGetValue(DirectionFieldName, out direction)
                              && (string)direction == DescendingEnumName;

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

        public FieldResolver CreateResolveEntityByKey(IEntityType entityType)
        {
            var entityParameterExpression
                = Expression.Parameter(entityType.ClrType, "entity");

            var variableExpressions = new List<ParameterExpression>();
            var blockExpressions = new List<Expression>();

            Expression predicateExpression = null;

            var keyArguments = new List<QueryArgument>();

            foreach (var keyProperty in entityType.FindPrimaryKey().Properties)
            {
                var keyPropertyVariableName = keyProperty.GraphQL().FieldName;
                var queryArgument = new QueryArgument(_typeMapper.FindMapping(keyProperty))
                {
                    Name = keyPropertyVariableName,
                    Description = keyProperty.GraphQL().Description
                };

                keyArguments.Add(queryArgument);

                var keyVariableExpression
                    = Expression.Variable(keyProperty.ClrType, keyPropertyVariableName);

                variableExpressions.Add(keyVariableExpression);

                var assignKeyVariableExpression
                    = Expression.Assign(
                        keyVariableExpression,
                        Expression.Call(
                            _getArgumentMethodInfo.MakeGenericMethod(keyProperty.ClrType),
                            _resolveFieldContextParameterExpression,
                            Expression.Constant(keyPropertyVariableName)));

                blockExpressions.Add(assignKeyVariableExpression);

                var equalExpression
                    = Expression.Equal(
                        Expression.Call(
                            null,
                            _efPropertyMethodInfo.MakeGenericMethod(keyProperty.ClrType),
                            entityParameterExpression,
                            Expression.Constant(keyProperty.Name)),
                        keyVariableExpression);

                predicateExpression
                    = predicateExpression == null
                        ? equalExpression
                        : Expression.AndAlso(predicateExpression, equalExpression);
            }

            Debug.Assert(predicateExpression != null);

            var queryEntityByKeyAsyncCallExpression
                = Expression.Call(
                    _queryEntityByKeyAsyncMethodInfo.MakeGenericMethod(entityType.ClrType),
                    _resolveFieldContextParameterExpression,
                    Expression.Quote(
                        Expression.Lambda(
                            predicateExpression,
                            entityParameterExpression)));

            blockExpressions.Add(queryEntityByKeyAsyncCallExpression);

            // TODO: Remove closure here
            var blockExpression
                = Expression.Block(variableExpressions, blockExpressions);

            var resolveLambdaExpression
                = Expression
                    .Lambda<Func<ResolveFieldContext, object>>(
                        blockExpression,
                        _resolveFieldContextParameterExpression);

            return new FieldResolver
            {
                Resolve = resolveLambdaExpression.Compile(),
                Arguments = new QueryArguments(keyArguments)
            };
        }

        public static MethodInfo _getArgumentMethodInfo
            = typeof(FieldResolverFactory).GetTypeInfo().GetDeclaredMethod(nameof(GetArgument));

        private static TArgument GetArgument<TArgument>(ResolveFieldContext resolveFieldContext, string name)
            => (TArgument)resolveFieldContext.Arguments[name];

        public static MethodInfo _queryEntityByKeyAsyncMethodInfo
            = typeof(FieldResolverFactory).GetTypeInfo().GetDeclaredMethod(nameof(QueryEntityByKeyAsync));

        private static Task<TEntity> QueryEntityByKeyAsync<TEntity>(
            ResolveFieldContext resolveFieldContext,
            Expression<Func<TEntity, bool>> predicate)
            where TEntity : class
            => (resolveFieldContext.Source as DbContext)?
                .Set<TEntity>()
                .SingleOrDefaultAsync(predicate, resolveFieldContext.CancellationToken);
    }
}

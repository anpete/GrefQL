using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using GraphQL.Types;
using GrefQL.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GrefQL.Query
{
    public class FieldResolverFactory : IFieldResolverFactory
    {
        public FieldResolverFactory(IGraphTypeMapper typeMapper)
        {
            _typeMapper = typeMapper;
        }

        private readonly IGraphTypeMapper _typeMapper;

        private static readonly ParameterExpression _resolveFieldContextParameterExpression
            = Expression.Parameter(typeof(ResolveFieldContext), "resolveFieldContext");

        private static readonly MethodInfo _efPropertyMethodInfo
            = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(Property));

        private static readonly List<QueryArgument> ListArguments = new List<QueryArgument>
        {
            new QueryArgument<IntGraphType>
            {
                Name = "offset"
            },
            new QueryArgument<IntGraphType>
            {
                Name = "limit"
            }
        };

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
                Arguments = new QueryArguments(ListArguments)
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

            TryApplyArgument<int>(resolveFieldContext, "offset", offset => query = query.Skip(offset));
            TryApplyArgument<int>(resolveFieldContext, "limit", limit => query = query.Take(limit));

            return query.ToArrayAsync(resolveFieldContext.CancellationToken);
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

            var keyArguments = new List<QueryArgument> { };

            foreach (var keyProperty in entityType.FindPrimaryKey().Properties)
            {
                var keyPropertyVariableName = keyProperty.GraphQL().NameOrDefault();
                var queryArgument = new QueryArgument(_typeMapper.FindMapping(keyProperty))
                {
                    Name = keyPropertyVariableName,
                    Description = keyProperty.GraphQL().DescriptionOrDefault()
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
                .SingleAsync(predicate, resolveFieldContext.CancellationToken);
    }
}

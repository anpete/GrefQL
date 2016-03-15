using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GrefQL
{
    public class FieldResolverFactory : IFieldResolverFactory
    {
        private static readonly MethodInfo _efPropertyMethodInfo
            = typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(Property));

        public Func<ResolveFieldContext, object> CreateResolveEntityByKey(IEntityType entityType)
        {
            var resolveFieldContextParameterExpression
                = Expression.Parameter(typeof(ResolveFieldContext), "resolveFieldContext");

            var entityParameterExpression
                = Expression.Parameter(entityType.ClrType, "entity");

            var variableExpressions = new List<ParameterExpression>();
            var blockExpressions = new List<Expression>();

            Expression predicateExpression = null;

            foreach (var keyProperty in entityType.FindPrimaryKey().Properties)
            {
                var keyPropertyVariableName = keyProperty.Name.ToCamelCase();

                var keyVariableExpression
                    = Expression.Variable(keyProperty.ClrType, keyPropertyVariableName);

                variableExpressions.Add(keyVariableExpression);

                var assignKeyVariableExpression
                    = Expression.Assign(
                        keyVariableExpression,
                        Expression.Call(
                            _getArgumentMethodInfo.MakeGenericMethod(keyProperty.ClrType),
                            resolveFieldContextParameterExpression,
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
                    resolveFieldContextParameterExpression,
                    Expression.Quote(
                        Expression.Lambda(
                            predicateExpression,
                            entityParameterExpression)));

            blockExpressions.Add(queryEntityByKeyAsyncCallExpression);

            var blockExpression
                = Expression.Block(variableExpressions, blockExpressions);

            var resolveLambdaExpression
                = Expression
                    .Lambda<Func<ResolveFieldContext, object>>(
                        blockExpression,
                        resolveFieldContextParameterExpression);

            return resolveLambdaExpression.Compile();
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

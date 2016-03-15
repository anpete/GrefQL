using System;
using System.Threading.Tasks;
using GraphQL;
using GrefQL.Query;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore
{
    public static class DbContextExtension
    {
        // TODO strong type query and variables
        public static ExecutionResult ExecuteGraphQLQuery(this DbContext context, string query, string variables = null)
            => context.ExecuteGraphQLQueryAsync(query, variables).GetAwaiter().GetResult();

        public static Task<ExecutionResult> ExecuteGraphQLQueryAsync(this DbContext context, string query, string variables = null)
        {
            var exectutor = context.GetInfrastructure().GetService<GraphQLExecutor>();
            if (exectutor == null)
            {
                throw new InvalidOperationException("This context is not enabled for GraphQL. Add GraphQL to context services with .AddGraphQL()");
            }
            return exectutor.ExecuteAsync(context, query, variables);
        }
    }
}

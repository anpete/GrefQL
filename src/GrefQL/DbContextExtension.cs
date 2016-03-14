using System;
using System.Threading.Tasks;
using GraphQL;

// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore
{
    public static class DbContextExtension
    {
        // TODO strong type query and variables
        public static ExecutionResult ExecuteGraphQLQuery(this DbContext context, string query, string variables)
            => new ExecutionResult
            {
                Errors = new ExecutionErrors
                {
                    new ExecutionError("Not implemented", new NotImplementedException())
                }
            };

        public static Task<ExecutionResult> ExecuteGraphQLQueryAsync(this DbContext context, string query, string variables)
            => Task.FromResult(context.ExecuteGraphQLQuery(query, variables));
    }
}

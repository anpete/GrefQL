using System;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.EntityFrameworkCore;

namespace GrefQL
{
    public static class DbContextExtension
    {
        // TODO strong type query and variables
        public static ExecutionResult FromGraphQLQuery(this DbContext context, string query, string variables)
            => new ExecutionResult
            {
                Errors = new ExecutionErrors
                {
                    new ExecutionError("Not implemented", new NotImplementedException())
                }
            };

        public static Task<ExecutionResult> FromGraphQLQueryAsync(this DbContext context, string query, string variables)
            => Task.FromResult(context.FromGraphQLQuery(query, variables));
    }
}

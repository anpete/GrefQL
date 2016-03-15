using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;

// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore
{
    public static class DbContextExtension
    {
        // TODO strong type query and variables
        public static ExecutionResult ExecuteGraphQLQuery(this DbContext context, string query, string variables)
            => context.ExecuteGraphQLQueryAsync(query, variables).GetAwaiter().GetResult();

        public static Task<ExecutionResult> ExecuteGraphQLQueryAsync(this DbContext context, string query, string variables)
        {
            var documentExecutor = new DocumentExecuter();
            var schema = context.Model[GraphQLAnnotationNames.Schema] as Schema;
            return documentExecutor.ExecuteAsync(schema, context, query, null);
        }
    }
}

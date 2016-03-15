using System;
using System.Threading;
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

        public static async Task<ExecutionResult> ExecuteGraphQLQueryAsync(this DbContext context, string query, string variables)
        {
            var documentExecutor = new DocumentExecuter();
            // TODO this is gross but it solves object disposed error that happens on the second call to this
            var graph = context.Model[GraphQLAnnotationNames.Schema] as Graph;
            ExecutionResult result;
            _executeLock.WaitOne();
            graph.DbContext = context;
            result = await documentExecutor.ExecuteAsync(graph, null, query, null);
            graph.DbContext = null;
            _executeLock.Release();
            return result;
        }

        private static Semaphore _executeLock = new Semaphore(1,1);
    }
}

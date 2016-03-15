using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GrefQL.Query
{
    public class GraphQLExecutor
    {
        private readonly ISchema _schema;
        private readonly IDocumentExecuter _executor;

        public GraphQLExecutor(ISchema schema, IDocumentExecuter executor)
        {
            _schema = schema;
            _executor = executor;
        }

        public Task<ExecutionResult> ExecuteAsync(
            DbContext context,
            string query,
            string variables = null,
            CancellationToken cancellationToken = default(CancellationToken))
            => _executor.ExecuteAsync(
                schema: _schema,
                root: context,
                query: query,
                operationName: null,
                inputs: variables?.ToInputs(),
                cancellationToken: cancellationToken);
    }
}

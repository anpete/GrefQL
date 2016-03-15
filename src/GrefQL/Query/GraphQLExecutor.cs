using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GrefQL.Query
{
    public class GraphQLExecutor
    {
        private readonly ISchema _schema;

        public GraphQLExecutor(ISchema schema)
        {
            _schema = schema;
        }

        public Task<ExecutionResult> ExecuteAsync(DbContext context, string query, string variables)
        {
            var documentExecutor = new DocumentExecuter();
            // TODO how to variables get passed in?
            return documentExecutor.ExecuteAsync(_schema, context, query, null);
        }
    }
}

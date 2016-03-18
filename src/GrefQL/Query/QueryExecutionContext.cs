using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace GrefQL.Query
{
    public class QueryExecutionContext
    {
        public QueryExecutionContext()
        {
            ContextSemaphore = new SemaphoreSlim(1);
        }

        public DbContext DbContext { get; set; }

        public SemaphoreSlim ContextSemaphore { get; }
    }
}

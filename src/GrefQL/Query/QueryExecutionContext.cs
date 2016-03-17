using Microsoft.EntityFrameworkCore;

namespace GrefQL.Query
{
    public class QueryExecutionContext
    {
        public DbContext DbContext { get; set; }
    }
}

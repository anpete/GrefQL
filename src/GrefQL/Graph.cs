using GraphQL.Types;

namespace Microsoft.EntityFrameworkCore
{
    public class Graph : Schema
    {
        public DbContext DbContext { get; set; }
    }
}
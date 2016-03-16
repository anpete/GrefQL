using System;
using GraphQL.Types;

namespace GrefQL.Query
{
    public class FieldResolver
    {
        public Func<ResolveFieldContext, object> Resolve { get; set; }
        public QueryArguments Arguments { get; set; }
    }
}

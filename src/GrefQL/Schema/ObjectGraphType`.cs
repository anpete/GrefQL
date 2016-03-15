using GraphQL.Types;

namespace GrefQL.Schema
{
    public class ObjectGraphType<T> : ObjectGraphType
    {
        public ObjectGraphType()
        {
            IsTypeOf = v => v is T;
        }
    }
}

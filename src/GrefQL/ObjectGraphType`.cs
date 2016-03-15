using GraphQL.Types;

namespace GrefQL
{
    public class ObjectGraphType<T> : ObjectGraphType
    {
        public ObjectGraphType()
        {
            IsTypeOf = v => v is T;
        }
    }
}

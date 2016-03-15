using GraphQL.Types;

namespace GrefQL.Metadata
{
    public class ObjectGraphType<T> : ObjectGraphType
    {
        public ObjectGraphType()
        {
            IsTypeOf = v => v is T;
        }
    }
}

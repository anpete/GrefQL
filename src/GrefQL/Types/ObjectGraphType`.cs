using GraphQL.Types;

namespace GrefQL.Schema
{
    public class ObjectGraphType<T> : ObjectGraphType
    {
        public ObjectGraphType()
        {
            Name = typeof (T).Name;
            Description = typeof (T).FullName;
            //IsTypeOf = v => v is T;
        }
    }
}

using GraphQL.Types;

namespace GrefQL.Schema
{
    public class LongGraphType : ScalarGraphType
    {
        public LongGraphType()
        {
            Name = "Long";
        }

        public override object Coerce(object value)
        {
            long result;
            if (long.TryParse(value.ToString(), out result))
            {
                return result;
            }
            return null;
        }
    }
}
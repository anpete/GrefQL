namespace System
{
    public static class TypeExtensions
    {
        public static Type UnwrapNullableType(this Type type)
            => Nullable.GetUnderlyingType(type) ?? type;
    }
}

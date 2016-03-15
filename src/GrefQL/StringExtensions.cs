using Newtonsoft.Json.Serialization;

namespace GrefQL
{
    public static class StringExtensions
    {
        private static readonly CamelCasePropertyNamesContractResolver _resolver =
            new CamelCasePropertyNamesContractResolver();

        public static string ToCamelCase(this string str)
            => _resolver.GetResolvedPropertyName(str);
    }
}

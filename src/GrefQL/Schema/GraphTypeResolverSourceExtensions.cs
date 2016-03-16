using System;
using GraphQL.Types;

namespace GrefQL.Schema
{
    public static class GraphTypeResolverSourceExtensions
    {
        public static void AddResolver<T>(this IGraphTypeResolverSource source, Func<GraphType> resolver)
            => source.AddResolver(typeof(T), resolver);

        public static GraphType Resolve<T>(this IGraphTypeResolverSource source)
            => source.Resolve(typeof(T));
    }
}

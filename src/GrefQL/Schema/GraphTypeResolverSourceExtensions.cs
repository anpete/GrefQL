using System;
using GraphQL.Types;

namespace GrefQL.Schema
{
    public static class GraphTypeResolverSourceExtensions
    {
        public static void AddResolver<T>(this IGraphTypeResolverSource source, Func<T> resolver)
            where T : GraphType
            => source.AddResolver(typeof(T), resolver);

        public static T Resolve<T>(this IGraphTypeResolverSource source)
            where T : GraphType
            => source.Resolve(typeof(T)) as T;

        public static bool TryResolve<T>(this IGraphTypeResolverSource source, out T graphType)
            where T : GraphType
        {
            graphType = default(T);
            if (!source.Contains(typeof (T)))
            {
                return false;
            }
            graphType = source.Resolve<T>();
            return true;
        }
    }
}

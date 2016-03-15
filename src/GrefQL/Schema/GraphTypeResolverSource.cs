using System;
using System.Collections.Concurrent;
using GraphQL.Types;

namespace GrefQL.Metadata
{
    public class GraphTypeResolverSource : IGraphTypeResolverSource
    {
        private readonly ConcurrentDictionary<Type, Func<GraphType>> _cache
            = new ConcurrentDictionary<Type, Func<GraphType>>();

        public void AddResolver(Type type, Func<GraphType> resolver) 
            => _cache.GetOrAdd(type, resolver);

        public GraphType Resolve(Type type)
        {
            Func<GraphType> action;
            _cache.TryGetValue(type, out action);
            return action?.Invoke();
        }
    }
}

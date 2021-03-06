﻿using System;
using System.Collections.Concurrent;
using GraphQL.Types;
using GrefQL.Types;

namespace GrefQL.Schema
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
            if (action != null)
            {
                return action.Invoke();
            }
            if (!typeof(GraphType).IsAssignableFrom(type))
            {
                type = typeof(ObjectGraphType<>).MakeGenericType(type);
            }
            return (GraphType)Activator.CreateInstance(type);
        }

        public bool Contains(Type type) => _cache.ContainsKey(type);
    }
}

using System;
using GraphQL.Types;

namespace GrefQL
{
    public interface IGraphTypeResolverSource
    {
        void AddResolver(Type type, Func<GraphType> resolver);
        GraphType Resolve(Type type);
    }
}
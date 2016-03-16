using System;
using GraphQL.Types;

namespace GrefQL.Schema
{
    public interface IGraphTypeResolverSource
    {
        void AddResolver(Type type, Func<GraphType> resolver);
        GraphType Resolve(Type type);
    }
}

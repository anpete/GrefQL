using System;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GrefQL.Query
{
    public interface IFieldResolverFactory
    {
        Func<ResolveFieldContext, object> CreateResolveEntityByKey(IEntityType entityType);
    }
}

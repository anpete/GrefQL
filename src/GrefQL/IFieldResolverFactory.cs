using System;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GrefQL
{
    public interface IFieldResolverFactory
    {
        Func<ResolveFieldContext, object> CreateResolveEntityByKey(IEntityType entityType);
    }
}

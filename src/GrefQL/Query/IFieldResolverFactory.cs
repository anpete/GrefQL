using Microsoft.EntityFrameworkCore.Metadata;

namespace GrefQL.Query
{
    public interface IFieldResolverFactory
    {
        FieldResolver CreateResolveEntityByKey(IEntityType entityType);
        FieldResolver CreateResolveEntityList(IEntityType entityType);
    }
}

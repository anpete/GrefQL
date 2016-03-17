using Microsoft.EntityFrameworkCore.Metadata;

namespace GrefQL.Query
{
    public interface IFieldResolverFactory
    {
        FieldResolver CreateResolveEntityList(IEntityType entityType);
    }
}

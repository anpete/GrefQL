using Microsoft.EntityFrameworkCore.Metadata;

namespace GrefQL.Schema
{
    public interface IGraphSchemaFactory
    {
        GraphQL.Types.Schema Create(IModel model);
    }
}

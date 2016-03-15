using GraphQL.Types;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GrefQL.Metadata
{
    public interface IGraphSchemaFactory
    {
        Schema Create(IModel model);
    }
}

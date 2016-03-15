using GraphQL.Types;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GrefQL
{
    public interface IGraphSchemaFactory
    {
        Schema Create(IModel model);
    }
}

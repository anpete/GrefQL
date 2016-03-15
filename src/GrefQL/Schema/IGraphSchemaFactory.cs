using GraphQL.Types;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GrefQL.Schema
{
    public interface IGraphSchemaFactory
    {
        ISchema Create(IModel model);
    }
}

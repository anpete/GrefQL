using System;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GrefQL.Schema
{
    public interface IGraphTypeMapper
    {
        /// <summary>
        ///     Map <see cref="IProperty.ClrType" /> to an appropriate <see cref="GraphType" />
        /// </summary>
        /// <param name="property"></param>
        /// <param name="notNull"></param>
        /// <returns>typeof <see cref="GraphType" /></returns>
        Type FindMapping(IProperty property, bool notNull = false);
    }
}

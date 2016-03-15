using System;
using System.Collections.Generic;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GrefQL
{
    public class GraphTypeMapper : IGraphTypeMapper
    {
        private static readonly Dictionary<Type, Type> _map = new Dictionary<Type, Type>
        {
            { typeof (int), typeof (IntGraphType) },
            { typeof (string), typeof (StringGraphType) },
            { typeof (double), typeof (FloatGraphType) },
            { typeof (bool), typeof (BooleanGraphType) },
            { typeof (DateTime), typeof (DateGraphType) }
        };

        public Type FindMapping(IProperty property, bool notNull = false)
        {
            Type mapping;
            _map.TryGetValue(property.ClrType, out mapping);
            if ((mapping != null) && notNull)
            {
                return typeof (NonNullGraphType<>).MakeGenericType(mapping);
            }
            return mapping;
        }
    }
}

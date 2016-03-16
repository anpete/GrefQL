using System;
using System.Collections.Generic;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GrefQL.Schema
{
    public class GraphTypeMapper : IGraphTypeMapper
    {
        private static readonly Dictionary<Type, Type> _map = new Dictionary<Type, Type>
        {
            { typeof (ushort), typeof (IntGraphType) },
            { typeof (short), typeof (IntGraphType) },
            { typeof (uint), typeof (IntGraphType) },
            { typeof (int), typeof (IntGraphType) },
            { typeof (long), typeof (LongGraphType) },
            { typeof (ulong), typeof (LongGraphType) },
            { typeof (string), typeof (StringGraphType) },
            { typeof (double), typeof (FloatGraphType) },
            { typeof (float), typeof (FloatGraphType) },
            { typeof (bool), typeof (BooleanGraphType) },
            { typeof (DateTime), typeof (DateGraphType) },
            { typeof (DateTimeOffset), typeof (DateGraphType) },
        };

        public Type FindMapping(IProperty property)
        {
            Type mapping;
            _map.TryGetValue(property.ClrType.UnwrapNullableType(), out mapping);
            if ((mapping != null) && !property.IsNullable)
            {
                return typeof (NonNullGraphType<>).MakeGenericType(mapping);
            }
            return mapping;
        }
    }
}

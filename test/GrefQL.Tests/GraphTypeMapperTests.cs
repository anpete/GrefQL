using System;
using GraphQL.Types;
using GrefQL.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace GrefQL.Tests
{
    public class GraphTypeMapperTests
    {
        [Theory]
        [InlineData(typeof(NonNullGraphType<IntGraphType>), typeof(int), false)]
        [InlineData(typeof(IntGraphType), typeof(int?), true)]
        public void MapsTypes(Type expected, Type propertyType, bool isNullable)
        {
            var mapper = new GraphTypeMapper();
            var et = new EntityType("e", new Microsoft.EntityFrameworkCore.Metadata.Internal.Model(), ConfigurationSource.Explicit);
            var prop = new Property("intprop", et, ConfigurationSource.Explicit)
            {
                ClrType = propertyType,
                IsNullable = isNullable
            };

            Assert.Equal(expected, mapper.FindMapping(prop));
        }
    }
}

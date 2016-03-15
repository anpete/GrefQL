using GraphQL.Types;
using GrefQL.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace GrefQL.Tests
{
    public class GraphTypeMapperTests
    {
        [Fact]
        public void MapsGraphTypes()
        {
            var mapper = new GraphTypeMapper();
            var et = new EntityType("e", new Microsoft.EntityFrameworkCore.Metadata.Internal.Model(), ConfigurationSource.Explicit);
            var prop = new Property("intprop", et, ConfigurationSource.Explicit)
            {
                ClrType = typeof (int)
            };

            Assert.Equal(typeof (IntGraphType), mapper.FindMapping(prop));
        }

        [Fact]
        public void MapsNonNullGraphTypes()
        {
            var mapper = new GraphTypeMapper();
            var et = new EntityType("e", new Microsoft.EntityFrameworkCore.Metadata.Internal.Model(), ConfigurationSource.Explicit);
            var prop = new Property("intprop", et, ConfigurationSource.Explicit)
            {
                ClrType = typeof (int)
            };

            Assert.Equal(typeof (NonNullGraphType<IntGraphType>), mapper.FindMapping(prop, notNull: true));
        }
    }
}

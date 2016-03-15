using System;
using GraphQL.Types;
using GrefQL.Tests.Model.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Xunit;

namespace GrefQL.Tests
{
    public class GraphSchemaFactoryTests
    {
        [Fact]
        public void CreatesFieldsForEntityTypes()
        {
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity<Customer>();
            var factory = new GraphSchemaFactory(new GraphTypeMapper(), new TestFieldResolverFactory());

            var graph = factory.Create(modelBuilder.Model);

            Assert.Collection(graph.Query.Fields, customer =>
                {
                    Assert.Equal("customer", customer.Name);
                    Assert.Equal(typeof (ObjectGraphType), customer.Type);
                    var arg = Assert.Single(customer.Arguments);
                    Assert.Equal("customerId", arg.Name);
                    Assert.Equal(typeof (NonNullGraphType<StringGraphType>), arg.Type);
                });
        }
    }

    // TODO just for the sake of compiling
    public class TestFieldResolverFactory : IFieldResolverFactory
    {
        public Func<ResolveFieldContext, object> CreateResolveEntityByKey(IEntityType entityType)
            => _ => null;
    }
}

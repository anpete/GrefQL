﻿using System.Linq;
using GraphQL.Types;
using GrefQL.Query;
using GrefQL.Schema;
using GrefQL.Tests.Model.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Xunit;

namespace GrefQL.Tests
{
    public class GraphSchemaFactoryTests
    {
        [Fact]
        public void CreatesFieldsForProperties()
        {
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity<Customer>();
            var source = new GraphTypeResolverSource();
            var factory = new GraphSchemaFactory(new GraphTypeMapper(), new FieldResolverFactory(new GraphTypeMapper()), source);

            factory.Create(modelBuilder.Model);

            var graphType = source.Resolve<ObjectGraphType<Customer>>();
            Assert.NotNull(graphType);

            Assert.All(graphType.Fields, f => Assert.Null(f.Resolve));
            Assert.All(graphType.Fields, f =>
                {
                    if (f.Arguments != null)
                    {
                        Assert.Empty(f.Arguments);
                    }
                });

            var entityType = modelBuilder.Model.FindEntityType(typeof (Customer));
            Assert.Equal(entityType.GetProperties().Count(), graphType.Fields.Count());

            var id = Assert.Single(graphType.Fields, f => f.Name == "customerId");
            Assert.Equal(typeof (NonNullGraphType<StringGraphType>), id.Type);

            var company = Assert.Single(graphType.Fields, f => f.Name == "companyName");
            Assert.Equal(typeof (StringGraphType), company.Type);
        }

        [Fact]
        public void CreatesFieldsForEntityTypes()
        {
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity<Customer>();
            var factory = new GraphSchemaFactory(new GraphTypeMapper(), new FieldResolverFactory(new GraphTypeMapper()), new GraphTypeResolverSource());

            var graph = factory.Create(modelBuilder.Model);

            Assert.Collection(graph.Query.Fields,
                customer =>
                    {
                        Assert.Equal("customer", customer.Name);
                        Assert.Equal(typeof (ObjectGraphType<Customer>), customer.Type);
                        Assert.NotEmpty(customer.Arguments);
                        Assert.NotNull(customer.Resolve);
                    },
                list =>
                    {
                        Assert.Equal("customers", list.Name);
                        Assert.Equal(typeof (ListGraphType<ObjectGraphType<Customer>>), list.Type);
                        Assert.NotEmpty(list.Arguments);
                        Assert.NotNull(list.Resolve);
                    });
        }
    }
}

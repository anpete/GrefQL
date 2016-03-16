using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using GrefQL.Query;
using GrefQL.Schema;
using GrefQL.Tests.Model.Northwind;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace GrefQL.Tests
{
    public class FieldResolverFactoryTests : NorthwindTestsBase
    {
        [Fact]
        public async Task Create_resolver_when_entity_has_simple_key()
        {
            using (var context = CreateContext())
            {
                var customerType = context.Model.FindEntityType(typeof(Customer));

                Assert.NotNull(customerType);

                var fieldResolverFactory = new FieldResolverFactory(new GraphTypeMapper());

                var resolver = fieldResolverFactory.CreateResolveEntityByKey(customerType).Resolve;

                var resolveFieldContext = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object> { ["customerId"] = "ALFKI" },
                    Source = context
                };

                var customer = await (Task<Customer>)resolver(resolveFieldContext);

                Assert.Equal("ALFKI", customer.CustomerId);
            }
        }

        [Fact]
        public async Task Create_resolver_when_entity_has_composite_key()
        {
            using (var context = CreateContext())
            {
                var orderDetailType = context.Model.FindEntityType(typeof(OrderDetail));

                Assert.NotNull(orderDetailType);

                var fieldResolverFactory = new FieldResolverFactory(new GraphTypeMapper());

                var resolver = fieldResolverFactory.CreateResolveEntityByKey(orderDetailType).Resolve;

                var resolveFieldContext = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object>
                    {
                        ["orderId"] = 10248,
                        ["productId"] = 11
                    },
                    Source = context
                };

                var orderDetail = await (Task<OrderDetail>)resolver(resolveFieldContext);

                Assert.Equal(10248, orderDetail.OrderId);
                Assert.Equal(11, orderDetail.ProductId);
            }
        }

        [Fact]
        public async Task Create_resolver_for_entities()
        {
            using (var context = CreateContext())
            {
                var customerType = context.Model.FindEntityType(typeof(Customer));

                Assert.NotNull(customerType);

                var fieldResolverFactory = new FieldResolverFactory(new GraphTypeMapper());

                var resolver = fieldResolverFactory.CreateResolveEntityList(customerType).Resolve;

                var resolveFieldContext = new ResolveFieldContext
                {
                    Source = context
                };

                var customers = await (Task<Customer[]>)resolver(resolveFieldContext);

                Assert.Equal(91, customers.Length);
            }
        }

        [Fact]
        public async Task Create_resolver_for_entities_with_limit()
        {
            using (var context = CreateContext())
            {
                var customerType = context.Model.FindEntityType(typeof(Customer));

                Assert.NotNull(customerType);

                var fieldResolverFactory = new FieldResolverFactory(new GraphTypeMapper());

                var resolver = fieldResolverFactory.CreateResolveEntityList(customerType).Resolve;

                var resolveFieldContext = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object> { ["limit"] = 10 },
                    Source = context
                };

                var customers = await (Task<Customer[]>)resolver(resolveFieldContext);

                Assert.Equal(10, customers.Length);
            }
        }

        [Fact]
        public async Task Create_resolver_for_entities_with_offset()
        {
            using (var context = CreateContext())
            {
                var customerType = context.Model.FindEntityType(typeof(Customer));

                Assert.NotNull(customerType);

                var fieldResolverFactory = new FieldResolverFactory(new GraphTypeMapper());

                var resolver = fieldResolverFactory.CreateResolveEntityList(customerType).Resolve;

                var resolveFieldContext = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object> { ["offset"] = 90 },
                    Source = context
                };

                var customers = await (Task<Customer[]>)resolver(resolveFieldContext);

                Assert.Equal(1, customers.Length);
            }
        }

        [Fact]
        public async Task Create_resolver_for_entities_with_ordering()
        {
            using (var context = CreateContext())
            {
                var customerType = context.Model.FindEntityType(typeof(Customer));

                Assert.NotNull(customerType);

                var fieldResolverFactory = new FieldResolverFactory(new GraphTypeMapper());

                var resolver = fieldResolverFactory.CreateResolveEntityList(customerType).Resolve;

                var resolveFieldContext = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object>
                    {
                        ["orderBy"] = new object[]
                        {
                            new Dictionary<string, object> { ["field"] = "companyName" }
                        }
                    },
                    Source = context
                };

                var customers = await (Task<Customer[]>)resolver(resolveFieldContext);

                Assert.Equal(91, customers.Length);
                Assert.Equal("Alfreds Futterkiste", customers.First().CompanyName);
                Assert.Equal("Wolski  Zajazd", customers.Last().CompanyName);
            }
        }

        [Fact]
        public async Task Create_resolver_for_entities_with_ordering_descending()
        {
            using (var context = CreateContext())
            {
                var customerType = context.Model.FindEntityType(typeof(Customer));

                Assert.NotNull(customerType);

                var fieldResolverFactory = new FieldResolverFactory(new GraphTypeMapper());

                var resolver = fieldResolverFactory.CreateResolveEntityList(customerType).Resolve;

                var resolveFieldContext = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object>
                    {
                        ["orderBy"] = new object[]
                        {
                            new Dictionary<string, object>
                            {
                                ["field"] = "companyName",
                                ["direction"] = "DESC"
                            }
                        }
                    },
                    Source = context
                };

                var customers = await (Task<Customer[]>)resolver(resolveFieldContext);

                Assert.Equal(91, customers.Length);
                Assert.Equal("Wolski  Zajazd", customers.First().CompanyName);
                Assert.Equal("Alfreds Futterkiste", customers.Last().CompanyName);
            }
        }

        [Fact]
        public async Task Create_resolver_for_entities_with_multiple_orderings()
        {
            using (var context = CreateContext())
            {
                var customerType = context.Model.FindEntityType(typeof(Customer));

                Assert.NotNull(customerType);

                var fieldResolverFactory = new FieldResolverFactory(new GraphTypeMapper());

                var resolver = fieldResolverFactory.CreateResolveEntityList(customerType).Resolve;

                var resolveFieldContext = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object>
                    {
                        ["orderBy"] = new object[]
                        {
                            new Dictionary<string, object>
                            {
                                ["field"] = "city",
                                ["direction"] = "ASC"
                            },
                            new Dictionary<string, object>
                            {
                                ["field"] = "companyName",
                                ["direction"] = "DESC"
                            }
                        }
                    },
                    Source = context
                };

                var customers = await (Task<Customer[]>)resolver(resolveFieldContext);

                Assert.Equal(91, customers.Length);
                Assert.Equal("DRACD", customers.First().CustomerId);
                Assert.Equal("WOLZA", customers.Last().CustomerId);
            }
        }

        public FieldResolverFactoryTests(NorthwindFixture northwindFixture, ITestOutputHelper testOutputHelper)
            : base(northwindFixture)
        {
            SetTestOutputHelper(testOutputHelper);
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Types;
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

                var fieldResolverFactory = new FieldResolverFactory();

                var resolver = fieldResolverFactory.CreateResolveEntityByKey(customerType);

                var resolveFieldContext = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object> { ["CustomerId"] = "ALFKI" },
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

                var fieldResolverFactory = new FieldResolverFactory();

                var resolver = fieldResolverFactory.CreateResolveEntityByKey(orderDetailType);

                var resolveFieldContext = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object>
                    {
                        ["OrderId"] = 10248,
                        ["ProductId"] = 11
                    },
                    Source = context
                };

                var orderDetail = await (Task<OrderDetail>)resolver(resolveFieldContext);

                Assert.Equal(10248, orderDetail.OrderId);
                Assert.Equal(11, orderDetail.ProductId);
            }
        }

        public FieldResolverFactoryTests(NorthwindFixture northwindFixture, ITestOutputHelper testOutputHelper)
            : base(northwindFixture, testOutputHelper)
        {
        }
    }
}

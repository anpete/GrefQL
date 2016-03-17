using System.Collections.Generic;
using GraphQL.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace GrefQL.Tests
{
    // TODO: Result verification

    public class QueryTests : NorthwindTestsBase
    {
        [Fact]
        public void Query_customers_by_id()
        {
            const string query = @"
                query Customer {
                  customers(customerId: 'ALFKI') {
                    customerId
                    companyName
                    contactName
                  }
                }";

            using (var context = CreateContext())
            {
                var result = context.ExecuteGraphQLQuery(query);

                Assert.Null(result.Errors);
                Assert.NotNull(result.Data);

                var jsonResult = new DocumentWriter().Write(result);

                WriteLine();
                WriteLine(jsonResult);
            }
        }

        [Fact]
        public void Query_customers_with_limit()
        {
            const string query = @"
                {
                  customers(limit: 10) {
                    customerId
                    companyName
                    contactName
                  }
                }";

            using (var context = CreateContext())
            {
                var result = context.ExecuteGraphQLQuery(query);

                Assert.Null(result.Errors);
                Assert.NotNull(result.Data);

                var jsonResult = new DocumentWriter().Write(result);

                WriteLine();
                WriteLine(jsonResult);
            }
        }

        [Fact]
        public void Query_customers_with_order_by()
        {
            const string query = @"
                {
                  customers(orderBy: [{ field: 'companyName', direction: DESC }]) {
                    customerId
                    companyName
                    contactName
                  }
                }";

            using (var context = CreateContext())
            {
                var result = context.ExecuteGraphQLQuery(query);

                Assert.Null(result.Errors);
                Assert.NotNull(result.Data);

                var jsonResult = new DocumentWriter().Write(result);

                WriteLine();
                WriteLine(jsonResult);
            }
        }

        [Fact]
        public void Query_customers_by_company_name()
        {
            const string query = @"
                {
                  customers(contactTitle: ""Sales Representative"") {
                    customerId
                    companyName
                    contactName
                  }
                }";

            using (var context = CreateContext())
            {
                var result = context.ExecuteGraphQLQuery(query);

                Assert.Null(result.Errors);
                Assert.NotNull(result.Data);

                var jsonResult = new DocumentWriter().Write(result);

                WriteLine();
                WriteLine(jsonResult);
            }
        }
        
        [Fact]
        public void Query_customers_by_company_name_and_city()
        {
            const string query = @"
                {
                  customers(contactTitle: ""Sales Representative"", city: ""London"") {
                    customerId
                    companyName
                    contactName
                  }
                }";

            using (var context = CreateContext())
            {
                var result = context.ExecuteGraphQLQuery(query);

                Assert.Null(result.Errors);
                Assert.NotNull(result.Data);

                var jsonResult = new DocumentWriter().Write(result);

                WriteLine();
                WriteLine(jsonResult);
            }
        }


        [Fact]
        public void Query_customer_with_orders()
        {
            const string query = @"
                {
                  customers(customerId: ""ALFKI"") {
                    customerId
                    companyName
                    contactName
                    orders {
                        orderId
                        orderDate   
                    }
                  }
                }";

            using (var context = CreateContext())
            {
                var result = context.ExecuteGraphQLQuery(query);

                Assert.Null(result.Errors);
                Assert.NotNull(result.Data);

                var jsonResult = new DocumentWriter().Write(result);

                WriteLine();
                WriteLine(jsonResult);
            }
        }


        [Fact]
        public void Query_orders_with_customer()
        {
            const string query = @"
                {
                   orders(limit: 2) {
                      orderId
                      customer {
                          customerName
                          contactName
                      }
                   }
                }";

            using (var context = CreateContext())
            {
                var result = context.ExecuteGraphQLQuery(query);

                Assert.Null(result.Errors);
                Assert.NotNull(result.Data);

                var jsonResult = new DocumentWriter().Write(result);

                WriteLine();
                WriteLine(jsonResult);
            }
        }

        [Fact]
        public void Introspection()
        {
            const string query = @"{
                   __schema { 
                     types {
                        name
                     }   
                   }
            } ";

            using (var context = CreateContext())
            {
                var schema = context.ExecuteGraphQLQuery(query);
                Assert.Null(schema.Errors);
                dynamic data = schema.Data;
                var result = Assert.IsType<object[]>(data["__schema"]["types"]);
                var names = new HashSet<string>();
                foreach (var t in result)
                {
                    names.Add(t["name"]);
                }
                Assert.Contains("Order", names);
                Assert.Contains("Customer", names);
            }
        }

        public QueryTests(NorthwindFixture northwindFixture, ITestOutputHelper testOutputHelper)
            : base(northwindFixture)
        {
            SetTestOutputHelper(testOutputHelper);
        }
    }
}

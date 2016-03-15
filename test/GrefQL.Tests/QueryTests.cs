using GraphQL;
using GraphQL.Http;
using GrefQL.Tests.Model.Northwind;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace GrefQL.Tests
{
    public class QueryTests : NorthwindTestsBase
    {
        [Fact]
        public void Query_customers_by_id()
        {
            const string query = @"
                query Customer {
                  customer(customerId: 'ALFKI') {
                    customerId
                    companyName
                    contactName
                  }
                }";

            using (var data = CreateContext())
            {
                var result = data.ExecuteGraphQLQuery(query);

                Assert.Null(result.Errors);

                var jsonResult = new DocumentWriter().Write(result);

                WriteLine();
                WriteLine(jsonResult);
            }
        }

        [Fact]
        public void Sandbox()
        {
            const string query = @"
                {
                  customers {
                    customerId
                    companyName
                    contactName
                  }
                }";

            using (var data = CreateContext())
            {
                var schema = new NorthwindGraph();
                var documentExecutor = new DocumentExecuter();

                var result = documentExecutor.ExecuteAsync(schema, data, query, null).Result;

                Assert.Null(result.Errors);

                var jsonResult = new DocumentWriter().Write(result);

                WriteLine();
                WriteLine(jsonResult);
            }
        }

        public QueryTests(NorthwindFixture northwindFixture, ITestOutputHelper testOutputHelper)
            : base(northwindFixture, testOutputHelper)
        {
        }
    }
}

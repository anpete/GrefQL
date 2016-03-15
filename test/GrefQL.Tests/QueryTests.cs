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
        public void Hello_world()
        {
            const string query = @"
                query CustomerNameQuery {
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

        public QueryTests(NorthwindFixture northwindFixture, ITestOutputHelper testOutputHelper)
            : base(northwindFixture, testOutputHelper)
        {
        }
    }
}

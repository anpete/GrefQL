using GraphQL.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace GrefQL.Tests
{
    public class ConcurrentQueryTest : NorthwindTestsBase
    {
        [Fact(Skip = "https://github.com/anpete/GrefQL/issues/12")]
        public void ItDoesntThrowConcurrencyError()
        {
            const string query = @"{
              customers(limit:1) {
                address
                city
                contactName
                phone
                orders {
                  orderId
                  customer {
                    customerId
                  }
                }
              }
            }
";
            using (var context = CreateContext())
            {
                var result = context.ExecuteGraphQLQuery(query);

                Assert.NotNull(result.Data);
                Assert.Null(result.Errors);

                var jsonResult = new DocumentWriter().Write(result);
                var expected = @"{""data"":{""customers"":[{""address"":""Obere Str. 57"",""city"":""Berlin"",""contactName"":""Maria Anders"",""phone"":""030-0074321"",""orders"":[{""orderId"":10643,""customer"":{""customerId"":""ALFKI""}},{""orderId"":10692,""customer"":{""customerId"":""ALFKI""}},{""orderId"":10702,""customer"":{""customerId"":""ALFKI""}},{""orderId"":10835,""customer"":{""customerId"":""ALFKI""}},{""orderId"":10952,""customer"":{""customerId"":""ALFKI""}},{""orderId"":11011,""customer"":{""customerId"":""ALFKI""}}]}]}}";

                Assert.Equal(expected, jsonResult, ignoreWhiteSpaceDifferences: true);

                WriteLine();
                WriteLine(jsonResult);
            }
        }

        public ConcurrentQueryTest(NorthwindFixture northwindFixture, ITestOutputHelper output)
            : base(northwindFixture)
        {
            SetTestOutputHelper(output);
        }
    }
}

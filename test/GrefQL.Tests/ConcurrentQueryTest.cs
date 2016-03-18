using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace GrefQL.Tests
{
    public class ConcurrentQueryTest : NorthwindTestsBase
    {
        [Fact]
        public void It_doesnt_throw_concurrency_error()
        {
            const string query = @"{
              customers(limit:1) {
                customerId
                address
                city
                contactName
                phone
                orders {
                  orderId
                  customerId
                  customer {
                    customerId
                  }
                }
              }
            }";

            using (var context = CreateContext())
            {
                var result = context.ExecuteGraphQLQuery(query);

                Assert.NotNull(result.Data);
                Assert.Null(result.Errors);
            }
        }

        public ConcurrentQueryTest(NorthwindFixture northwindFixture, ITestOutputHelper output)
            : base(northwindFixture)
        {
            SetTestOutputHelper(output);
        }
    }
}

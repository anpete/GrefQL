using GraphQL;
using GraphQL.Http;
using GrefQL.Tests.Model.Northwind;
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
            var factory = new GraphSchemaFactory(new GraphTypeMapper(), new FieldResolverFactory(), new GraphTypeResolverSource());

            using (var data = CreateContext())
            {
                var schema = factory.Create(data.Model);
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

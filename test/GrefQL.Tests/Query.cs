using GraphQL;
using GraphQL.Http;
using GrefQL.Tests.Model.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace GrefQL.Tests
{
    public class Query
    {
        [Fact]
        public void HelloWorld()
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
                var schema = new NorthwindGraph(data);
                var documentExecutor = new DocumentExecuter();

                var result = documentExecutor.ExecuteAsync(schema, null, query, null).Result;

                Assert.Null(result.Errors);

                var jsonResult = new DocumentWriter().Write(result);

                WriteLine();
                WriteLine(jsonResult);
            }
        }

        private NorthwindContext CreateContext()
        {
            var serviceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            loggerFactory.AddProvider(new TestOutputHelperLoggerProvider(_testOutputHelper));

            var options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(serviceProvider)
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Northwind;Trusted_Connection=True;");

            return new NorthwindContext(options.Options);
        }

        private readonly ITestOutputHelper _testOutputHelper;

        public Query(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private void WriteLine(object s = null) => _testOutputHelper.WriteLine(s?.ToString() ?? "");
    }
}

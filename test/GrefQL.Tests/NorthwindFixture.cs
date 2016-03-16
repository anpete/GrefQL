using GrefQL.Tests.Model.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace GrefQL.Tests
{
    public class NorthwindFixture
    {
        private readonly TestOutputHelperLoggerProvider _testOutputHelperLoggerProvider = new TestOutputHelperLoggerProvider();
        private readonly DbContextOptions _options;

        public NorthwindFixture()
        {
            var serviceProvider 
                = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddGraphQL()
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            loggerFactory.AddProvider(_testOutputHelperLoggerProvider);

            var contextOptionsBuilder
                = new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(serviceProvider)
                    .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Northwind;Trusted_Connection=True;")
                    .EnableSensitiveDataLogging();

            _options = contextOptionsBuilder.Options;
        }

        public NorthwindContext CreateContext() => new NorthwindContext(_options);

        public void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelperLoggerProvider.TestOutputHelper = testOutputHelper;
        }
    }
}

using System;
using GrefQL.Tests.Model.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace GrefQL.Tests
{
    public class NorthwindFixture
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;

        public NorthwindFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .AddGraphQL()
                    .BuildServiceProvider();

            var contextOptionsBuilder
                = new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Northwind;Trusted_Connection=True;")
                    .EnableSensitiveDataLogging();

            _options = contextOptionsBuilder.Options;
        }

        public NorthwindContext CreateContext() => new NorthwindContext(_options);

        public void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
        {
            var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();

            loggerFactory.AddProvider(new TestOutputHelperLoggerProvider(testOutputHelper));
        }
    }
}

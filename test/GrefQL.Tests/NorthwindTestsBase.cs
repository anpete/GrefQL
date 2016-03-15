using GrefQL.Tests.Model.Northwind;
using Xunit;
using Xunit.Abstractions;

namespace GrefQL.Tests
{
    public abstract class NorthwindTestsBase : IClassFixture<NorthwindFixture>
    {
        private readonly NorthwindFixture _northwindFixture;
        private readonly ITestOutputHelper _testOutputHelper;

        protected NorthwindTestsBase(NorthwindFixture northwindFixture, ITestOutputHelper testOutputHelper)
        {
            _northwindFixture = northwindFixture;
            _testOutputHelper = testOutputHelper;
            _northwindFixture.SetTestOutputHelper(testOutputHelper);
        }

        protected NorthwindContext CreateContext() => _northwindFixture.CreateContext();

        protected void WriteLine(object s = null) => _testOutputHelper.WriteLine(s?.ToString() ?? "");
    }
}

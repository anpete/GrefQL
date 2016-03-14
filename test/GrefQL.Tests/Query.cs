using System.Linq;
using GrefQL.Tests.Model;
using Xunit;

namespace GrefQL.Tests
{
    public class Query
    {
        [Fact]
        public void HelloWorld()
        {
            using (var context = new StarWarsContext())
            {
                Assert.Equal(2, context.Humans.ToList().Count);
            }
        }
    }
}

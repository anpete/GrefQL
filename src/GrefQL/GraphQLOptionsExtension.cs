using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GrefQL
{
    public class GraphQLOptionsExtension : IDbContextOptionsExtension
    {
        public void ApplyServices(IServiceCollection builder) 
            => builder.AddGraphQL();
    }
}
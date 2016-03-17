using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GrefQL
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder EnableGraphQL(this DbContextOptionsBuilder optionsBuilder)
        {
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(GetOrCreateExtension(optionsBuilder));
            return optionsBuilder;
        }

        private static GraphQLOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder) 
            => optionsBuilder.Options.FindExtension<GraphQLOptionsExtension>() ?? new GraphQLOptionsExtension();
    }
}

using Microsoft.EntityFrameworkCore;

namespace GrefQL.Tests.Model.Northwind
{
    public class NorthwindContext : DbContext
    {
        public NorthwindContext(DbContextOptions options)
            :base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customers");
            // TODO don't require user to manually add annotation
            // TODO don't hang the GraphQL model off the IModel
            modelBuilder.HasAnnotation(GraphQLAnnotationNames.Schema, new NorthwindGraph(this));
        }

        public DbSet<Customer> Customers { get; set; }
    }
}

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
        }
    }
}

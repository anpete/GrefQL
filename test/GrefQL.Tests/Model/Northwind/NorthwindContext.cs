using Microsoft.EntityFrameworkCore;

namespace GrefQL.Tests.Model.Northwind
{
    public class NorthwindContext : DbContext
    {
        public NorthwindContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Customer>()
                .ToTable("Customers");

            modelBuilder
                .Entity<Order>()
                .ToTable("Orders");

            modelBuilder
                .Entity<OrderDetail>()
                .ToTable("Order Details")
                .HasKey(od => new { od.OrderId, od.ProductId });

            modelBuilder
                .Entity<Product>()
                .ToTable("Products");

            modelBuilder
                .Entity<Employee>()
                .ToTable("Employees");

            // TODO don't hang the GraphQL model off the IModel
            // TODO use DI
            var factory = new GraphSchemaFactory(new GraphTypeMapper(), new FieldResolverFactory(), new GraphTypeResolverSource());
            modelBuilder.HasAnnotation(GraphQLAnnotationNames.Schema, factory.Create(modelBuilder.Model));
        }

        public DbSet<Customer> Customers { get; set; }
    }
}

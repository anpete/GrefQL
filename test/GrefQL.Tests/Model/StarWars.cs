using System;
using Microsoft.EntityFrameworkCore;

namespace GrefQL.Tests.Model
{
    public class StarWarsContext : DbContext
    {
        public StarWarsContext(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GrephQL.StarWars;Trusted_Connection=True;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Character>();
            modelBuilder.Entity<Human>();
            modelBuilder.Entity<Droid>();
        }
    }

    public abstract class Character
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Human : Character
    {
        public string HomePlanet { get; set; }
    }

    public class Droid : Character
    {
        public string PrimaryFunction { get; set; }
    }
}

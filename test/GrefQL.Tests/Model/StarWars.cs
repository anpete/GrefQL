using Microsoft.EntityFrameworkCore;

namespace GrefQL.Tests.Model
{
    public class StarWarsContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GrephQL.StarWars;Trusted_Connection=True;");
        }

        public DbSet<Human> Humans { get; set; }
    }

    public class Human
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string HomePlanet { get; set; }
    }
}

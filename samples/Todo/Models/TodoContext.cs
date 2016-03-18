using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Todo.Models
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions options)
            :base(options)
        {
        }
            
        public DbSet<User> Users { get; set; }
        public DbSet<Todo> Todos { get; set; }
    }

    public class Node
    {
        public int Id { get; set; }
    }

    public class User : Node
    {
        public ICollection<Todo> Todos { get; set; }

        public int TotalCount => Todos.Count;
        public int CompletedCount => Todos.Count(t => t.Complete);
    }

    public class Todo : Node
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public string Text { get; set; }
        public bool Complete { get; set; }
    }
}
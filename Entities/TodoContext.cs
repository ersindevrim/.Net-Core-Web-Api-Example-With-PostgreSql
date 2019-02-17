using Microsoft.EntityFrameworkCore;
using Todo2Api.Models;

namespace Todo2Api.Entities
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options ) : base(options)
        {

        }

        public DbSet<TodoItem> TodoItems{get;set;}
        public DbSet<Users> Users{get;set;}
    }
}
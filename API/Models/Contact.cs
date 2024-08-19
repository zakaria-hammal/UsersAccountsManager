using Microsoft.EntityFrameworkCore;

namespace Projet.Models
{
    public class Contact : DbContext
    {
        public DbSet<ChatRoom> ChatRooms {get; set;}
        public DbSet<Message> Messages {get; set;}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Contact.db");
            
            optionsBuilder.UseSqlite($"Filename={path}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Projet.Models;

namespace Projet.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            List<AppRole> roles = new List<AppRole>
                {
                    new AppRole
                        {
                            Name = "Superuser",
                            NormalizedName = "SUPERUSER"
                        },

                    new AppRole 
                        {
                            Name = "Admin",
                            NormalizedName = "ADMIN"
                        },

                    new AppRole
                        {
                            Name = "Manager",
                            NormalizedName = "MANAGER"
                        },

                    new AppRole
                        {
                            Name = "Employee",
                            NormalizedName = "EMPLOYEE"
                        },

                    new AppRole
                        {
                            Name = "Client",
                            NormalizedName = "CLIENT"
                        }
                };

            builder.Entity<AppRole>().HasData(roles);

            PasswordHasher<string> pw = new PasswordHasher<string>();

            var user = new AppUser
                {
                    UserName = "Administrator",
                    NormalizedUserName = "ADMINISTRATOR",
                    Email = "admin@email.com",
                    NormalizedEmail = "ADMIN@GMAIL.COM",
                    Role = "Superuser"
                };

            user.PasswordHash = pw.HashPassword("Administrator", "Admin_Password_123456789");

            builder.Entity<AppUser>().HasData(user);
        }
    }
}

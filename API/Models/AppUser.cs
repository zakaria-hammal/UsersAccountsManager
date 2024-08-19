using Microsoft.AspNetCore.Identity;

namespace Projet.Models
{
    public class AppUser : IdentityUser
    {
        public string? Role{get; set;}
        public string? RefreshToken{get; set;}
        public DateTime? RefreshTokenExpiry{get; set;}
        public int OpenedSessions{get; set;}

    }
}

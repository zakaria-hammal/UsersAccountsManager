using System.Security.Claims;

namespace Projet.Models
{
    public class RoleAddModel
    {
        public string? Rolename {get; set;}
        public List<ClaimModel>? Claims {get; set;}
    }
}
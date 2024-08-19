using System.Security.Claims;

namespace MyApp
{
    public class RoleAddModel
    {
        public string? Rolename {get; set;}
        public List<ClaimModel>? Claims {get; set;}
    }
}
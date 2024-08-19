using System.Security.Claims;

namespace CLIENT
{
    public class RoleAddModel
    {
        public string? Rolename {get; set;}
        public List<ClaimModel>? Claims {get; set;}
    }
}
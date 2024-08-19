using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Projet.Models;

namespace Projet.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;

        public TokenService(IConfiguration config, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
        {
            _config = config;
#pragma warning disable CS8604 // Possible null reference argument.
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
#pragma warning restore CS8604 // Possible null reference argument.

            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<string> CreateToken(AppUser user)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.GivenName, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("role", user.Role),
                };
#pragma warning restore CS8604 // Possible null reference argument.

            var role = _roleManager.Roles.FirstOrDefault(r => r.Name == user.Role);

#pragma warning disable CS8604 // Possible null reference argument.
            var cls = await _userManager.GetClaimsAsync(user);
#pragma warning restore CS8604 // Possible null reference argument.

            claims.AddRange(cls);

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddHours(12),
                    SigningCredentials = creds,
                    Issuer = _config["Jwt:Issuer"],
                    Audience = _config["Jwt:Audience"]
                };
            
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

    }
}

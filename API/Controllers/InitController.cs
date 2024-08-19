using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet.Services;
using Projet.Models.Account;
using Projet.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace Projet.Controllers
{
    [Authorize(Roles = "Superuser", AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class InitController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        public InitController(UserManager<AppUser> userManager, TokenService tokenService, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<IActionResult> Initialization()
        {
            var text = System.IO.File.ReadAllText("Init.json");

            bool initialized = JsonSerializer.Deserialize<bool>(text);

            try
            {
                if(!initialized)
                {
                    var Superuser = _roleManager.Roles.FirstOrDefault(r => r.Name == "Superuser");
                    var Admin = _roleManager.Roles.FirstOrDefault(r => r.Name == "Admin");
                    var Manager = _roleManager.Roles.FirstOrDefault(r => r.Name == "Manager");
                    var Employee = _roleManager.Roles.FirstOrDefault(r => r.Name == "Employee");
                    var Client = _roleManager.Roles.FirstOrDefault(r => r.Name == "Client");

                    var AdminClaims = new List<Claim>
                        {
                            new Claim("Can Add Role", "Can Add Role"),
                            new Claim("Can Add Admin", "Can Add Admin"),
                            new Claim("Can Add Claim", "Can Add Claim"),
                            new Claim("Can Add User", "Can Add User"),
                            new Claim("Can Add Product", "Can Add Product"),
                            new Claim("Can View Role", "Can View Role"),
                            new Claim("Can View Admin", "Can View Admin"),
                            new Claim("Can View Claim", "Can View Claim"),
                            new Claim("Can View User", "Can View User"),
                            new Claim("Can View Product", "Can View Product"),
                            new Claim("Can DeleteOrLock User", "Can DeleteOrLock User")
                        };

                    var ManagerClaims = new List<Claim>
                        {
                            new Claim("Can Add User", "Can Add User"),
                            new Claim("Can Add Product", "Can Add Product"),
                            new Claim("Can View Admin", "Can View Admin"),
                            new Claim("Can View Role", "Can View Role"),
                            new Claim("Can View Claim", "Can View Claim"),
                            new Claim("Can View User", "Can View User"),
                            new Claim("Can View Product", "Can View Product")
                        };

                    var EmployeeClaims = new List<Claim>
                        {
                            new Claim("Can View Admin", "Can View Admin"),
                            new Claim("Can Add Product", "Can Add Product"),
                            new Claim("Can View User", "Can View User"),
                            new Claim("Can View Product", "Can View Product")
                        };

                    var ClientClaims = new List<Claim>
                        {
                            new Claim("Can View Product", "Can View Product")
                        };

    #pragma warning disable CS8604 // Possible null reference argument.
                    foreach (var item in AdminClaims)
                    {
                        await _roleManager.AddClaimAsync(Admin, item);
                        await _roleManager.AddClaimAsync(Superuser, item);
                    }

                    await _roleManager.AddClaimAsync(Superuser, new Claim("Can Delete Admin","Can Delete Admin"));

                    foreach (var item in ManagerClaims)
                    {
                        await _roleManager.AddClaimAsync(Manager, item);
                    }

                    foreach (var item in EmployeeClaims)
                    {
                        await _roleManager.AddClaimAsync(Employee, item);
                    }

                    foreach (var item in ClientClaims)
                    {
                        await _roleManager.AddClaimAsync(Client, item);
    #pragma warning restore CS8604 // Possible null reference argument.
                    }

                    var ad = await _userManager.Users.FirstOrDefaultAsync(u => u.Role == "Superuser");

                    var SuperuserClaims = AdminClaims;

                    SuperuserClaims.Add(new Claim("Can Delete Admin","Can Delete Admin"));

#pragma warning disable CS8604 // Possible null reference argument.
                    await _userManager.AddClaimsAsync(ad, SuperuserClaims);
                    await _userManager.AddToRoleAsync(ad, "Superuser");
#pragma warning restore CS8604 // Possible null reference argument.
                    System.IO.File.Delete("Init.json");
                    System.IO.File.WriteAllText("Init.json",  JsonSerializer.Serialize(true));
                }
                
                return Ok(initialized);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] string Username)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(Username);
                
                if (user == null)
                    return NotFound("Username not found");

                Directory.Delete(Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),"Users_Images"), user.Id), true);

                using (var db1 = new Contact())
                {
                    var rooms = db1.ChatRooms.Where(r => r.User1 == user.Id || r.User2 == user.Id);
                    
                    foreach (var item in rooms)
                    {
                        var messages = db1.Messages.Where(m => m.RoomId == item.RoomId);
                        db1.Messages.RemoveRange(messages);
                        db1.SaveChanges();
                    }

                    db1.ChatRooms.RemoveRange(rooms);
                    db1.SaveChanges();
                    
                }

                await _userManager.RemoveFromRoleAsync(user, user.Role);

                var userClaims = await _userManager.GetClaimsAsync(user);

                await _userManager.RemoveClaimsAsync(user, userClaims);

                var result = _userManager.DeleteAsync(user);

                return Ok($"{user.UserName} deleted successfully");
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
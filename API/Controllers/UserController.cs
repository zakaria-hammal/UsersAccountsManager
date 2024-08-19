using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projet.Data;
using Projet.Models;
using Projet.Services;

namespace Projet.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        
        public UserController(UserManager<AppUser> userManager, TokenService tokenService, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _userManager.Users;

            var acUsers = new List<UserModel>();

            foreach (var item in users)
            {
                var user = new UserModel
                    {
                        UserId = item.Id,
                        UserName = item.UserName,
                        Email = item.Email,
                        Role = item.Role,
                        Two_Factor_Auth = item.TwoFactorEnabled
                    };

                acUsers.Add(user);
            }

            return Ok(acUsers);
        }

        [Authorize(Policy = "Can_View_Role",  AuthenticationSchemes = "Bearer")]
        [HttpGet]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.ToList();

            var rls = new List<String>();

            foreach (var item in roles)
            {
                var s = item.Name;

#pragma warning disable CS8604 // Possible null reference argument.
                rls.Add(s);
#pragma warning restore CS8604 // Possible null reference argument.
            }

            return Ok(rls);
        }

        [Authorize(Policy = "Can_view_Claim",  AuthenticationSchemes = "Bearer")]
        [HttpGet]
        public IActionResult GetClaims()
        {
            var json = System.IO.File.ReadAllText("Claims.json");

            var claims = JsonSerializer.Deserialize<List<ClaimModel>>(json);

            return Ok(claims);
        }

        [Authorize(Policy = "Can_Add_Role", AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] RoleAddModel model)
        {
            var role = new AppRole
                {
                    Name = model.Rolename,
                    NormalizedName = model.Rolename.ToUpper()
                };

            await _roleManager.CreateAsync(role);

            foreach (var item in model.Claims)
            {
                await _roleManager.AddClaimAsync(role, new Claim(item.Type, item.Value));
            }

            await _roleManager.UpdateAsync(role);

            return Ok();
        }

        [Authorize(Policy = "Can_Add_Role", AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public async Task<IActionResult> DeleteRole([FromBody] string roleToDelete)
        {
            var role = await _roleManager.FindByNameAsync(roleToDelete);

            var users = await _userManager.GetUsersInRoleAsync(roleToDelete);

            foreach (var item in users)
            {
                Directory.Delete(Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),"Users_Images"), item.Id), true);

                using (var db1 = new Contact())
                {
                    var rooms = db1.ChatRooms.Where(r => r.User1 == item.Id || r.User2 == item.Id);
                    
                    foreach (var item1 in rooms)
                    {
                        var messages = db1.Messages.Where(m => m.RoomId == item1.RoomId);
                        db1.Messages.RemoveRange(messages);
                        db1.SaveChanges();
                    }

                    db1.ChatRooms.RemoveRange(rooms);
                    db1.SaveChanges();
                    
                }

                await _userManager.RemoveFromRoleAsync(item, item.Role);

                var userClaims = await _userManager.GetClaimsAsync(item);

                await _userManager.RemoveClaimsAsync(item, userClaims);

                var result = await _userManager.DeleteAsync(item);
            }

            await _roleManager.DeleteAsync(role);

            return Ok();
        }

        [Authorize(Policy = "Can_Add_Role", AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public async Task<IActionResult> ChangeRole([FromBody] RoleChange roleChange)
        {
            var user = await _userManager.FindByNameAsync(roleChange.User.UserName);

            if(roleChange.NewRole == "Client")
            {
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
            }
            else if(user.Role == "Client")
            {
                List<AppUser> users = _userManager.Users.ToList();

                    using (var db1 = new Contact())
                    {
                        List<ChatRoom> rooms = new();

                        foreach (var item in users)
                        {
                            var room = new ChatRoom 
                            {
                                RoomId = GenerateId(),
                                User1 =  user.Id,
                                User2 =  item.Id
                            };
                            rooms.Add(room);
                        }
                        
                        db1.ChatRooms.AddRange(rooms);
                        db1.SaveChanges();
                    }
            }

            var role = roleChange.User.Role;
            
            user.Role = roleChange.NewRole;
            var claims = await _userManager.GetClaimsAsync(user);
            await _userManager.RemoveClaimsAsync(user, claims);
            await _userManager.RemoveFromRoleAsync(user, role);

            await _userManager.AddToRoleAsync(user, roleChange.NewRole);
            var newRole = await _roleManager.FindByNameAsync(roleChange.NewRole);

            var newClaims = await _roleManager.GetClaimsAsync(newRole);
            await _userManager.AddClaimsAsync(user, newClaims);

            await _userManager.UpdateAsync(user);

            return Ok();
        }

        [NonAction]
        private static string GenerateId()
        {
            var random = new byte[64];

            using var generator = RandomNumberGenerator.Create();
            generator.GetBytes(random);

            return Convert.ToBase64String(random);
        }
    }
}

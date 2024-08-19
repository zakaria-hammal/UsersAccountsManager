using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projet.Models;
using Projet.Models.Account;
using Projet.Services;

namespace Projet.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class AdminController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;

        public AdminController(UserManager<AppUser> userManager, TokenService tokenService, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        
        [Authorize(Policy = "Can_Add_Admin",  AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public async Task<IActionResult> AddAdmin([FromBody] RegisterModel registerModel)
        {
            
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = new AppUser
                    {
                        UserName = registerModel.Username,
                        Email = registerModel.Email,
                        Role = "Admin",
                        OpenedSessions = 0,
                        TwoFactorEnabled = registerModel.Two_Factor_Auth
                    };

#pragma warning disable CS8604 // Possible null reference argument.
                var createUser = await _userManager.CreateAsync(user, registerModel.Password);

                if (createUser.Succeeded)
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

                    var filePath = Path.Combine("Users_Images", user.Id);

                    Directory.CreateDirectory($"Users_Images/{user.Id}");
                    
                    var role = await _userManager.AddToRoleAsync(user, "Admin");
                    
                    var r = await _roleManager.FindByNameAsync("Admin");

                    var claims = await _roleManager.GetClaimsAsync(r);
#pragma warning restore CS8604 // Possible null reference argument.

                    await _userManager.AddClaimsAsync(user, claims);

                    if(role.Succeeded)
                    {
                        return Ok("Admin Added successfully");
                    }
                    else
                    {
                        return StatusCode(500, role.Errors);
                    }
                }
                else
                {
                    return StatusCode(500, createUser.Errors);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
    
        }

        [Authorize(Policy = "Can_Add_User", AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] RegisterModel registerModel)
        {
            
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = new AppUser
                    {
                        UserName = registerModel.Username,
                        Email = registerModel.Email,
                        Role = registerModel.Role,
                        OpenedSessions = 0,
                        TwoFactorEnabled = registerModel.Two_Factor_Auth
                    };

#pragma warning disable CS8604 // Possible null reference argument.
                var createUser = await _userManager.CreateAsync(user, registerModel.Password);

                if (createUser.Succeeded)
                {
                    if(registerModel.Role != "Client")
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

                    
                    var filePath = Path.Combine("Users_Images", user.Id);

                    Directory.CreateDirectory($"Users_Images/{user.Id}");

                    var role = await _userManager.AddToRoleAsync(user, registerModel.Role );

                    if(role.Succeeded)
                    {
                        
                    var r = await _roleManager.FindByNameAsync(registerModel.Role );
                
                    var cls = await _roleManager.GetClaimsAsync(r);
#pragma warning restore CS8604 // Possible null reference argument.

                    await _userManager.AddClaimsAsync(user, cls);
                        return Ok($"{registerModel.Role} Added successfully");
                    }
                    else
                    {
                        return StatusCode(500, role.Errors);
                        
                    }
                }
                else
                {
                    return StatusCode(500, createUser.Errors);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
    
        }

        [Authorize(Policy = "Can_DeleteOrLock_User",  AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public async Task<IActionResult> LockoutUser([FromBody] LockoutModel lockout)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

#pragma warning disable CS8604 // Possible null reference argument.
                var user = await _userManager.FindByNameAsync(lockout.UserName);
#pragma warning restore CS8604 // Possible null reference argument.

                if (user == null)
                    return NotFound("Username not found");

                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;

                var result = _userManager.SetLockoutEnabledAsync(user, true);
                var end = _userManager.SetLockoutEndDateAsync(user, lockout.EndDate);

                return Ok($"{user.UserName} locked out successfully");
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [Authorize(Policy = "Can_DeleteOrLock_User",  AuthenticationSchemes = "Bearer")]
        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] string Username)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(Username);

                if (user == null)
                    return NotFound("Username not found");

                if (user.Role == "Admin")
                    return Forbid("You can't delete an admin");
                
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

                var result = await _userManager.DeleteAsync(user);

                return Ok($"{user.UserName} deleted successfully");
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
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

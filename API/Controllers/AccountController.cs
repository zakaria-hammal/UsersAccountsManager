using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet.Services;
using Projet.Models.Account;
using Projet.Models;
using System.Security.Claims;
using System.Text.Json;
using System.Security.Cryptography;

namespace Projet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ISenderEmail _emailSender;

        public AccountController(UserManager<AppUser> userManager, TokenService tokenService, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, ISenderEmail emailSender)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == loginModel.UserName);

            if (user == null)
                return NotFound("Username not found and/or password incorrect");

#pragma warning disable CS8604 // Possible null reference argument.
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password, true);
#pragma warning restore CS8604 // Possible null reference argument.

            if (!result.Succeeded)
                return NotFound("Username not found and/or password incorrect");

            user.OpenedSessions++;

            if(user.RefreshToken == null || user.RefreshTokenExpiry < DateTime.Now)
            {
                user.RefreshToken = GenerateRefreshToken();
                user.RefreshTokenExpiry = DateTime.Now.AddDays(30);
            }

            await _userManager.UpdateAsync(user);

            return Ok(user.RefreshToken);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refresh)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refresh);

            if(user != null)
            {
                user.OpenedSessions--;
                
                if (user.OpenedSessions == 0)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiry = null;   
                }
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.SignOutAsync();    

            return Ok("Sign out successfull");
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _emailSender.SendEmailAsync(registerModel.Email, "Confirm", $"This is Your confirmation code {registerModel.Confirmation_Code.ToString()}");
            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            try
            {
                var text = System.IO.File.ReadAllText("Init.json");

                bool initialized = JsonSerializer.Deserialize<bool>(text);
                
                if (initialized == false)
                {
                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = new AppUser
                    {
                        UserName = registerModel.Username,
                        Email = registerModel.Email,
                        Role = "Client",
                        OpenedSessions = 0,
                        TwoFactorEnabled = registerModel.Two_Factor_Auth
                    };
                
#pragma warning disable CS8604 // Possible null reference argument.
                var createUser = await _userManager.CreateAsync(user, registerModel.Password);

                if (createUser.Succeeded)
                {
                    var filePath = Path.Combine("Users_Images", user.Id);

                    Directory.CreateDirectory($"Users_Images/{user.Id}");

                    var role = await _userManager.AddToRoleAsync(user, "Client");

                    var r = await _roleManager.FindByNameAsync("Client");

                    var claims = await _roleManager.GetClaimsAsync(r);
#pragma warning restore CS8604 // Possible null reference argument.

                    await _userManager.AddClaimsAsync(user, claims);

#pragma warning disable CS8604 // Possible null reference argument.
                    await _emailSender.SendEmailAsync(user.Email, "Welcome", $"Welcome {user.UserName} to our Application", false);
#pragma warning restore CS8604 // Possible null reference argument.

                    if (role.Succeeded)
                    {
                        return Ok($"Welcome {user.UserName}");
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

        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangeModel changeModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == changeModel.Username);

#pragma warning disable CS8604 // Possible null reference argument.
            var result = await _userManager.ChangePasswordAsync(user, changeModel.CurrentPassword, changeModel.NewPassword);
#pragma warning restore CS8604 // Possible null reference argument

            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            return Ok();
        }
        
        [HttpPost("updateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] ChangeInfo model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(model.CurrentUsername);

            if (!String.IsNullOrEmpty(model.NewUsername))
            {
                user.UserName = model.NewUsername;
                user.NormalizedUserName = model.NewUsername.ToUpper();    
            }
            
            if (!String.IsNullOrEmpty(model.NewEmail))
            {
                user.Email = model.CurrentEmail;
                user.NormalizedEmail = model.NewEmail.ToUpper();
            }

            user.TwoFactorEnabled = model.Two_Factor_Auth;

            await _userManager.UpdateAsync(user);
            return Ok();
        }

        [HttpPost("GetJwtToken")]
        public async Task<IActionResult> GetToken([FromBody] string refresh)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refresh);

            if (user == null || user.RefreshTokenExpiry < DateTime.Now)
                return Unauthorized();

            return Ok(_tokenService.CreateToken(user).Result);
        }

        [NonAction]
        private string GenerateRefreshToken()
        {
            var random = new byte[64];

            using var generator = RandomNumberGenerator.Create();
            generator.GetBytes(random);

            return Convert.ToBase64String(random);
        }

    }
}

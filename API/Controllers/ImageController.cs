using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projet.Models;
using Projet.Services;
using System.IO;

namespace Projet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public ImageController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("{imageName}")]
        public async Task<IActionResult> GetImage(string imageName)
        {
            var user = await _userManager.FindByNameAsync(imageName);

            var name = user.Id + ".png";

            var imagePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),"Users_Images"), user.Id, name);

            if (!System.IO.File.Exists(imagePath))
            {
                imagePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),"Users_Images"), "default.png");
            }

            var imageBytes = System.IO.File.ReadAllBytes(imagePath);
            var contentType = GetContentType(imagePath);

            return File(imageBytes, contentType);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(ImageModel model)
        {
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var user = await _userManager.FindByNameAsync(model.Description);

            var filePath = Path.Combine("Users_Images", user.Id, $"{user.Id}.png");

#pragma warning disable CS8604 // Possible null reference argument.
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
#pragma warning restore CS8604 // Possible null reference argument.

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            return Ok(new { FilePath = filePath });
        }


        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }
    }
}
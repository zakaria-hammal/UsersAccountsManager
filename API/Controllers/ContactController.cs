using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projet.Models;

namespace Projet.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class ContactController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public ContactController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessages(string id)
        {
            using var db = new Contact();

            var users = id.Split('&');

            AppUser Sender = await _userManager.FindByNameAsync(users[0]);
            AppUser Receiver = await _userManager.FindByNameAsync(users[1]);

            var room = db.ChatRooms.FirstOrDefault(r => (r.User1 == Sender.Id && r.User2 == Receiver.Id) || (r.User2 == Sender.Id && r.User1 == Receiver.Id));
            
            var messages = db.Messages.Where(m => m.RoomId == room.RoomId).ToList();

            return Ok(messages);
        }

    }
}
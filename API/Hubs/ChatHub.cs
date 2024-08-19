using Projet.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;

namespace Projet.Hubs
{
    public class ChatHub : Hub
    {
        private readonly UserManager<AppUser> _userManager;

        public ChatHub(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ChatRoom?> JoinRoom(string sender, string receiver)
        {
            using var db = new Contact();
            List<ChatRoom>  _rooms = db.ChatRooms.ToList();

            AppUser? Sender = await _userManager.FindByNameAsync(sender);
            AppUser? Receiver = await _userManager.FindByNameAsync(receiver);

            var room = _rooms.FirstOrDefault(r => (r.User1 == Sender.Id && r.User2 == Receiver.Id) || (r.User2 == Sender.Id && r.User1 == Receiver.Id));
            if(room is not null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomId);

                return room;
            }

            return null;
        }

        public async Task SendMessage(string sender, string receiver, string message)
        {
            using var db = new Contact();
            List<ChatRoom>  _rooms = db.ChatRooms.ToList();
            AppUser? Sender = await _userManager.FindByNameAsync(sender);
            AppUser? Receiver = await _userManager.FindByNameAsync(receiver);

            var room = _rooms.FirstOrDefault(r => (r.User1 == Sender.Id && r.User2 == Receiver.Id) || (r.User2 == Sender.Id && r.User1 == Receiver.Id));

            var msg = new Message
                {   
                    SenderId = Sender.Id,
                    RoomId = room.RoomId,
                    MessageBody = message
                };

            db.Messages.Add(msg);
            db.SaveChanges();

            await Clients.Group(room.RoomId).SendAsync("ReceiveMessage", sender, message);
        }

        public async Task ChangeMessage(string sender, string receiver, string message)
        {
            using var db = new Contact();
            
            List<ChatRoom>  _rooms = db.ChatRooms.ToList();
            AppUser? Sender = await _userManager.FindByNameAsync(sender);
            AppUser? Receiver = await _userManager.FindByNameAsync(receiver);

            var room = _rooms.FirstOrDefault(r => (r.User1 == Sender.Id && r.User2 == Receiver.Id) || (r.User2 == Sender.Id && r.User1 == Receiver.Id));

            await Clients.Group(room.RoomId).SendAsync("ReceiveChangeMessage", sender, message);
        }

        public async Task Quit(string sender, string receiver)
        {
            using var db = new Contact();
            
            List<ChatRoom>  _rooms = db.ChatRooms.ToList();
            AppUser? Sender = await _userManager.FindByNameAsync(sender);
            AppUser? Receiver = await _userManager.FindByNameAsync(receiver);

            var room = _rooms.FirstOrDefault(r => (r.User1 == Sender.Id && r.User2 == Receiver.Id) || (r.User2 == Sender.Id && r.User1 == Receiver.Id));

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.RoomId);
        }
    }
}

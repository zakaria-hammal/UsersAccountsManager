using System.ComponentModel.DataAnnotations;

namespace Projet.Models
{
    public class ChatRoom
    {
        public int ChatRoomID {get; set;}

        [StringLength(64)]
        public string RoomId {get; set;}

        [StringLength(64)]
        public string User1 {get; set;}

        [StringLength(64)]
        public string User2 {get; set;}

    }
}
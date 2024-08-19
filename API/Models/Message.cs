using System.ComponentModel.DataAnnotations;

namespace Projet.Models
{
    public class Message
    {
        public int MessageID {get; set;}

        [StringLength(64)]
        public string SenderId {get; set;}

        [StringLength(64)]
        public string RoomId {get; set;}

        [StringLength(64)]
        public string MessageBody {get; set;}

    }
}
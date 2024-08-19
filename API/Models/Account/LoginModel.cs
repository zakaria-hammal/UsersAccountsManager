using System.ComponentModel.DataAnnotations;

namespace Projet.Models.Account
{
    public class LoginModel
    {
        [Required]
        public string? UserName{get; set;}

        [Required]
        public string? Password {get; set;}
    }
}

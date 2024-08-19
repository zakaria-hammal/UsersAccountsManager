using System.ComponentModel.DataAnnotations;

namespace Projet.Models.Account
{
    public class RegisterModel
    {
        [Required]
        public string? Username {get; set;}

        [Required]
        public string? Email {get; set;}

        [Required]
        public string? Password {get; set;}

        [Required]
        public string? Role {get; set;}

        [Required]
        public int Confirmation_Code {get; set;}

        [Required]
        public bool Two_Factor_Auth {get; set;}
    }
}

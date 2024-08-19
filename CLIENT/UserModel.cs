namespace CLIENT
{
    public class UserModel
    {
        public string? UserId{get; set;}
        public string? UserName{get; set;}
        public string? Email{get; set;}

        public string? Role{get; set;}

        public string? Password {get; set;}

        public bool Two_Factor_Auth {get; set;}
    }
}
namespace CLIENT
{
    public class ChangeInfo
    {
        public string? CurrentUsername{get; set;}
        public string? NewUsername{get; set;}
        public string? CurrentEmail{get; set;}
        public string? NewEmail{get; set;}
        public bool Two_Factor_Auth {get; set;}
    }
}
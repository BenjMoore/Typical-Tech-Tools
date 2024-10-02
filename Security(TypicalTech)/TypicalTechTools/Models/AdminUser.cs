namespace TypicalTechTools.Models
{
    public class AdminUser
    {
        public string UserName { get; set; } 
        public string Password { get; set; }
        public int UserID { get; set; }
        public int AccessLevel { get; set; }
        public string Role { get; set; } = "User";
        public string ReturnUrl { get; set; } = string.Empty;
    }
}

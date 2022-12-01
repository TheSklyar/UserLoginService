namespace UserLoginService.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = new byte[1];
        public byte[] PasswordSalt { get; set; } = new byte[1];
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenCreated { get; set; }
        public DateTime TokenExpires { get; set; }
    }
}

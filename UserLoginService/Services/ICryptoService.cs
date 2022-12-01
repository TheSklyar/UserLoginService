using UserLoginService.Models;

namespace UserLoginService.Services
{
    public interface ICryptoService
    {
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        public bool VerifyPasswordHash(string password, User user);
        public string CreateToken(User user);
        public RefreshToken GenerateRefreshToken();
    }
}

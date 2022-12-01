using UserLoginService.Models;

namespace UserLoginService.Services
{
    public interface IDBService
    {
        public Task<int?> CreateUserAsync(string userName, byte[] passwordHash, byte[] passwordSalt);
        public Task<bool> ExistsUserAsync(string userName);
        public Task<User> GetUserAsync(string userName);
        public Task<User> FindUserByTokenAsync(string token);
        public Task SetUserRefreshTokenAsync(User user, RefreshToken refreshToken);
    }
}

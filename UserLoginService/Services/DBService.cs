using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Npgsql;
using System.Runtime.ConstrainedExecution;
using UserLoginService.Models;

namespace UserLoginService.Services
{
    public class DBService : IDBService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public DBService(IConfiguration configuration) 
        {
            _configuration = configuration;
            _connectionString = _configuration.GetSection("AppSettings:ConnectionString").Value;
        }
        public async Task<int?> CreateUserAsync(string userName, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var conDB = new NpgsqlConnection(_connectionString))
            {
                var UserID = await conDB.ExecuteScalarAsync<int?>(@"insert 
into ""default"".""Users""(
""UserName"", ""PasswordHash"", ""PasswordSalt"", ""RefreshToken"", ""TokenCreated"", ""TokenExpires""
) values (
@UserName, @PasswordHash, @PasswordSalt, null, null, null
) returning ""UserID""
", new { UserName = userName, PasswordHash= passwordHash, PasswordSalt= passwordSalt });
                return UserID;
            }
        }

        public async Task<bool> ExistsUserAsync(string userName)
        {
            using (var conDB = new NpgsqlConnection(_connectionString))
            {
                var UserID = await conDB.ExecuteScalarAsync<int?>(@"select 
""UserID"" 
from ""default"".""Users"" 
where ""UserName""=@UserName
", new { UserName = userName });
                return UserID.HasValue;
            }
        }

        public async Task<User> FindUserByTokenAsync(string token)
        {
            using (var conDB = new NpgsqlConnection(_connectionString))
            {
                var Users = await conDB.QueryAsync<User>(@"select 
""UserID"", ""UserName"", ""PasswordHash"", ""PasswordSalt"", ""RefreshToken"", ""TokenCreated"", ""TokenExpires""
from ""default"".""Users"" 
where ""RefreshToken""=@RefreshToken
", new { RefreshToken = token });
                if (Users.FirstOrDefault() is null)
                {
                    throw new Exception("Token not found");
                }
                else
                {
                    return Users.First();
                }

            }
        }

        public async Task<User> GetUserAsync(string userName)
        {
            using (var conDB = new NpgsqlConnection(_connectionString))
            {
                var Users = await conDB.QueryAsync<User>(@"select 
""UserID"", ""UserName"", ""PasswordHash"", ""PasswordSalt"", ""RefreshToken"", ""TokenCreated"", ""TokenExpires""
from ""default"".""Users"" 
where ""UserName""=@UserName
", new { UserName = userName });
                if(Users.FirstOrDefault() is null)
                {
                    throw new Exception("User not found");
                }
                else
                {
                    return Users.First();
                }
                
            }
        }

        public async Task SetUserRefreshTokenAsync(User user, RefreshToken refreshToken)
        {
            using (var conDB = new NpgsqlConnection(_connectionString))
            {
                var UserID = await conDB.QueryAsync(@"update 
""default"".""Users""
set
""RefreshToken"" = @RefreshToken
, ""TokenCreated"" = @TokenCreated
, ""TokenExpires"" = @TokenExpires
where 
""UserID""=@UserID
", new { UserID = user.UserID, RefreshToken = refreshToken.Token, TokenCreated = refreshToken.Created, TokenExpires = refreshToken.Expires });
            }
        }
    }
}

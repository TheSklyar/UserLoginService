using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserLoginService.Models;
using UserLoginService.Services;

namespace UserLoginService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IDBService _dbService;
        private readonly ICryptoService _cryptoService;
        public AuthController(IConfiguration configuration, IDBService dbService, ICryptoService cryptoService)
        {
            _configuration = configuration;
            _dbService = dbService;
            _cryptoService = cryptoService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<int?>> RegisterAsync(UserCredentials request)
        {
            try
            {
                _cryptoService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
                var UserID = await _dbService.CreateUserAsync(request.UserName, passwordHash, passwordSalt);
                return Ok(UserID);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);//временное решение
            }
        }
        [HttpPost("login")]
        public async Task<ActionResult<string>> LoginAsync(UserCredentials request)
        {
            try
            {
                if (!(await _dbService.ExistsUserAsync(request.UserName)))
                {
                    return NotFound("User not found");
                }
                var user = await _dbService.GetUserAsync(request.UserName);
                if (!_cryptoService.VerifyPasswordHash(request.Password, user))
                {
                    return BadRequest("Wrong password");
                }
                string token = _cryptoService.CreateToken(user);
                await SetRefreshToken(user);
                return Ok(token);
            }
            catch(Exception ex) 
            {
                return BadRequest(ex.Message);//временное решение
            }
        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshTokenAsync()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                var user = await _dbService.FindUserByTokenAsync(refreshToken);
                if (user.TokenExpires < DateTime.Now)
                {
                    return Unauthorized("Token Expired");
                }
                string token = _cryptoService.CreateToken(user);
                await SetRefreshToken(user);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);//временное решение
            }
        }
        [HttpGet("info"), Authorize]
        public async Task<ActionResult<string>> InfoAsync()
        {
            try
            {
                var UserName = HttpContext.User.FindFirstValue(ClaimTypes.Name);
                var ApiVersion = HttpContext.User.FindFirstValue(ClaimTypes.Version);
                return Ok(new { UserName, ApiVersion });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);//временное решение
            }
        }
        private async Task SetRefreshToken(User user)
        {
            var refreshToken = _cryptoService.GenerateRefreshToken();
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.Expires
            };
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
            await _dbService.SetUserRefreshTokenAsync(user, refreshToken);
        }
    }
}

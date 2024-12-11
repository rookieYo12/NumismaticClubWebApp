using Microsoft.AspNetCore.Mvc;
using AuthApi.Models;
using AuthApi.Services;
using System.CodeDom.Compiler;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _usersService;
        private readonly TokenService _tokenService;

        public AuthController(UserService usersService, TokenService tokenService)
        {
            _usersService = usersService;
            _tokenService = tokenService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(AuthRequest authRequest)
        {
            if (string.IsNullOrWhiteSpace(authRequest.Name) ||
                string.IsNullOrWhiteSpace(authRequest.Password))
            {
                return BadRequest("Wrong fields.");
            }

            if (await _usersService.GetAsync(authRequest.Name) != null)
            {
                return BadRequest("User with this login already exists.");
            }

            await _usersService.CreateAsync(new User
            {
                Id = "",
                Name = authRequest.Name,
                Password = authRequest.Password,
                Role = UserRole.User
            });

            return StatusCode(201); // Created
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn(AuthRequest authRequest)
        {
            // Trying to recognize the user
            if (string.IsNullOrWhiteSpace(authRequest.Name) || 
                string.IsNullOrWhiteSpace(authRequest.Password))
            {
                return BadRequest("Wrong fields.");
            }
            
            var user = await _usersService.GetAsync(authRequest.Name);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            if (authRequest.Password != user.Password)
            {
                return Unauthorized("Wrong password.");
            }

            // If authentication is successful generate tokens
            var accessToken = _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update user
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(10);
            
            await _usersService.UpdateAsync(user.Name, user);

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using AuthApi.Models;
using AuthApi.Services;
using System.CodeDom.Compiler;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;

        public AuthController(UserService usersService, TokenService tokenService)
        {
            _userService = usersService;
            _tokenService = tokenService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(AuthRequest authRequest)
        {
            if (string.IsNullOrWhiteSpace(authRequest.Login) ||
                string.IsNullOrWhiteSpace(authRequest.Password))
            {
                return BadRequest("Wrong fields.");
            }

            if (await _userService.GetAsync(authRequest.Login) != null)
            {
                return BadRequest("User with this login already exists.");
            }

            await _userService.CreateAsync(new User
            {
                Id = "",
                Login = authRequest.Login,
                Password = authRequest.Password,
                Role = UserRole.User
            });

            return StatusCode(201); // Created
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn(AuthRequest authRequest)
        {
            // Trying to recognize the user
            if (string.IsNullOrWhiteSpace(authRequest.Login) || 
                string.IsNullOrWhiteSpace(authRequest.Password))
            {
                return BadRequest("Wrong fields.");
            }
            
            var user = await _userService.GetAsync(authRequest.Login);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            if (authRequest.Password != user.Password)
            {
                return Unauthorized("Wrong password.");
            }

            // If authentication is successful get payload information
            var claims = new List<Claim>
            {
                // TODO: use another claim keys
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

            // Generate tokens
            var accessToken = _tokenService.GenerateToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update user
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(2);
            
            await _userService.UpdateAsync(user.Login, user);

            return Ok(new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh(TokenModel refreshRequest)
        {
            if (string.IsNullOrWhiteSpace(refreshRequest.AccessToken) ||
                string.IsNullOrWhiteSpace(refreshRequest.RefreshToken))
            {
                return BadRequest("Wrong access or refresh token.");
            }

            try
            {
                // Check access token
                var principal = _tokenService.GetPrincipalFromExpiredToken(refreshRequest.AccessToken);

                // TODO: Name is null, contains in claims ???
                var user = await _userService.GetAsync(principal.Identity.Name);

                if (user == null)
                {
                    return BadRequest("Wrong access token.");
                }

                // Check refresh token
                if (user.RefreshToken != refreshRequest.RefreshToken ||
                    user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return BadRequest("Wrong refresh token.");
                }

                // Generate new tokens
                var newAccessToken = _tokenService.GenerateToken(principal.Claims.ToList());
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // Update user
                user.RefreshToken = newRefreshToken;
                await _userService.UpdateAsync(user.Login, user);

                // TODO: cookie?

                return Ok(new TokenModel
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}

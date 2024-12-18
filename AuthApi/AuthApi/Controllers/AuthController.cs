using Microsoft.AspNetCore.Mvc;
using AuthApi.Models;
using AuthApi.Services;
using System.CodeDom.Compiler;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;
        private readonly ProducerService _producer;

        public AuthController(UserService usersService, TokenService tokenService,
            ProducerService producer)
        {
            _userService = usersService;
            _tokenService = tokenService;
            _producer = producer;
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

            var newUser = new User
            {
                Id = "",
                Login = authRequest.Login,
                Password = authRequest.Password,
            };
            newUser.Roles = [UserRole.User];

            await _userService.CreateAsync(newUser);

            _producer.Produce("create-user-topic", JsonSerializer.Serialize(newUser.Id));

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
                new Claim("sub", user.Id)
            };    
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim("Role", role.ToString()));
            }

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

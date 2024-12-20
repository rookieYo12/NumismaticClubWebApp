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
        public async Task<IActionResult> SignUp(AuthInfo authRequest)
        {
            if (string.IsNullOrWhiteSpace(authRequest.Login) ||
                string.IsNullOrWhiteSpace(authRequest.Password))
            {
                return BadRequest("Wrong fields.");
            }

            if (await _userService.GetByLoginAsync(authRequest.Login) != null)
            {
                return BadRequest("User with this login already exists.");
            }

            // TODO: maybe encrypt password

            var newUser = new User
            {
                Id = "",
                Login = authRequest.Login,
                Password = authRequest.Password,
            };
            newUser.Roles = [UserRole.User];

            await _userService.CreateAsync(newUser);

            _producer.Produce("user-created", JsonSerializer.Serialize(newUser.Id));

            return StatusCode(201); // Created
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn(AuthInfo authRequest)
        {
            // Trying to recognize the user
            if (string.IsNullOrWhiteSpace(authRequest.Login) || 
                string.IsNullOrWhiteSpace(authRequest.Password))
            {
                return BadRequest("Wrong fields.");
            }
            
            var user = await _userService.GetByLoginAsync(authRequest.Login);

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
                new Claim("Sub", user.Id)
            };    
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim("Roles", role.ToString()));
            }

            // Generate tokens
            var accessToken = _tokenService.GenerateToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update user
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(2);
            
            await _userService.UpdateAsync(user.Id, user);

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
                // Check access token and get principals from it
                var principal = _tokenService.GetPrincipalFromExpiredToken(refreshRequest.AccessToken);

                // Get id from principal
                var userId = principal.Claims.FirstOrDefault(c => c.Type == "Sub").Value;

                // Get user by id
                var user = await _userService.GetByIdAsync(userId);

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
                await _userService.UpdateAsync(user.Id, user);

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

        [HttpPut("{id:length(24)}/edit-auth")]
        public async Task<IActionResult> Update(string id, AuthInfo newAuth)
        {
            // Id from jwt
            var userId = Request.Headers["UserId"].ToString();

            // Check that owner trying to change auth data
            if (userId != id)
            {
                return BadRequest("You cannot change auth data other users.");
            }

            var user = await _userService.GetByIdAsync(id);
            
            if (user is null)
            {
                return NotFound();
            }

            user.Login = newAuth.Login;
            user.Password = newAuth.Password;
            
            await _userService.UpdateAsync(user.Id, user);

            return NoContent();
        }

        // TODO: maybe check uncorrect role digits
        [HttpPut("{id:length(24)}/edit-roles")]
        public async Task<IActionResult> UpdateRoles(string id, UpdateRolesRequest request)
        {
            var user = await _userService.GetByIdAsync(id);
            
            if (user is null)
            {
                return NotFound();
            }

            user.Roles = request.Roles;

            await _userService.UpdateAsync(id, user);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}/delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var roles = Request.Headers["Roles"].ToString().Split(',');

            if (!roles.Contains("Admin"))
            {
                // Сheck that the user trying to delete own account
                var userId = Request.Headers["UserId"].ToString();

                if (userId != id)
                {
                    return BadRequest("User with the role 'User' cannot delete other users.");
                }
            }

            var user = await _userService.GetByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            await _userService.DeleteAsync(id);

            _producer.Produce("user-deleted", JsonSerializer.Serialize(id));

            return NoContent();
        }
    }
}

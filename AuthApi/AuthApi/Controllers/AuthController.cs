using Microsoft.AspNetCore.Mvc;
using AuthApi.Models;
using AuthApi.Services;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _usersService;

        public AuthController(UserService usersService)
        {
            _usersService = usersService;
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
            if (string.IsNullOrWhiteSpace(authRequest.Name) || 
                string.IsNullOrWhiteSpace(authRequest.Password))
            {
                return BadRequest("Wrong fields.");
            }
            
            var user = await _usersService.GetAsync(authRequest.Name);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (authRequest.Password != user.Password)
            {
                return BadRequest("Wrong password.");
            }

            return;
        }
    }
}

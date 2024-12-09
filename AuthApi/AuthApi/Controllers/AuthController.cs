using Microsoft.AspNetCore.Mvc;
using AuthApi.Models;
using AuthApi.Services;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UsersService _usersService;

        public AuthController(UsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(AuthInfo authInfo)
        {
            if (await _usersService.GetAsync(authInfo.Login) != null)
            {
                return BadRequest("User with this login already exists");
            }

            await _usersService.CreateAsync(new User
            {
                Id = "",
                Login = authInfo.Login,
                Password = authInfo.Password
            });

            return StatusCode(201); // Created
        }
    }
}

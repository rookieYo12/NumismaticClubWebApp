using Microsoft.AspNetCore.Mvc;
using UserApi.Services;
using UserApi.Models;

namespace UserApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;
        
        public UsersController(UsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpGet]
        public async Task<List<User>> Get() =>
            await _usersService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<User>> Get(string id)
        {
            // Fetch data from db
            var user = await _usersService.GetAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPut("{id:length(24)}/edit-profile")]
        public async Task<IActionResult> Update(string id, User updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return BadRequest("User ID mismatch.");
            }

            // Id from jwt
            var userId = Request.Headers["UserId"].ToString();

            // Check that owner trying to change profile data
            if (userId != id)
            {
                return BadRequest("You cannot change profile data other users.");
            }

            var user = await _usersService.GetAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            // If find user in db update it
            await _usersService.UpdateAsync(id, updatedUser);

            return NoContent();
        }
    }
}

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

        [HttpPost]
        public async Task<IActionResult> Post(User newUser)
        {
            await _usersService.CreateAsync(newUser);

            return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, User updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return BadRequest("User ID mismatch.");
            }

            var user = await _usersService.GetAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            // If find coin in db update it
            await _usersService.UpdateAsync(id, updatedUser);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _usersService.GetAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            await _usersService.RemoveAsync(id); // Delete data from db

            return NoContent();
        }
    }
}

using UserApi.Models;
using System.Text.Json;

namespace UserApi.Services
{
    public class ResponseProcessingService
    {
        private readonly UsersService _usersService;

        public ResponseProcessingService(UsersService usersService)
        {
            _usersService = usersService;
        }

        public async Task PocessResponse(string message)
        {
            var responseObj = JsonSerializer.Deserialize<Response>(message);


        }
    }
}

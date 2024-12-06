using UserApi.Models;
using System.Text.Json;

namespace UserApi.Services
{
    // Service processes Kafka messages
    public class RequestProcessingService
    {
        private readonly UsersService _usersService;
        private readonly ProducerService _producerService;

        public RequestProcessingService(UsersService usersService, 
            ProducerService producerService)
        {
            _usersService = usersService;
            _producerService = producerService;
        }

        public async Task Process(string message)
        {
            var request = JsonSerializer.Deserialize<Request>(message);

            // Fetch user from db
            var user = await _usersService.GetAsync(request.UserId);

            if (user == null)
            {
                Console.WriteLine($"User with ID {request.UserId} not found.");
                return;
            }

            user.IncremetRegisteredObjects();

            // Update user in db
            await _usersService.UpdateAsync(request.UserId, user);
            
            // Get update time
            DateTimeOffset now = DateTimeOffset.Now;
            string updateTime = now.ToString();

            var response = JsonSerializer.Serialize(new Response
            {
                CoinId = request.CoinId,
                UpdateTime = updateTime
            });

            _producerService.Produce(response);
        }
    }
}

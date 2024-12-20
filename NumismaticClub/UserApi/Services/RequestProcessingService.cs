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

        public async Task CreateUser(string messageValue)
        {
            var userId = JsonSerializer.Deserialize<string>(messageValue);

            await _usersService.CreateAsync(new User
            {
                Id = userId,
                Name = "User",
                Surname = $"{userId}"
            });
        }

        public async Task DeleteUser(string messageValue)
        {
            var userId = JsonSerializer.Deserialize<string>(messageValue);

            await _usersService.RemoveAsync(userId); // Delete data from db
        }

        public async Task AddRegisteredObject(string messageValue)
        {
            var request = JsonSerializer.Deserialize<CoinCreatedRequest>(messageValue);

            // Fetch user from db
            var user = await _usersService.GetAsync(request.UserId);

            if (user == null)
            {
                _producerService.Produce("coin-created", messageValue); // Return to queue
                throw new Exception("User not found.");
            }

            user.IncremetRegisteredObjects();

            // Update user in db
            await _usersService.UpdateAsync(request.UserId, user);
            
            // Get update time
            DateTimeOffset now = DateTimeOffset.Now;
            string updateTime = now.ToString();

            var response = JsonSerializer.Serialize(new CoinConfirmedResponse
            {
                CoinId = request.CoinId,
                UpdateTime = updateTime
            });

            _producerService.Produce("coin-confirmed", response);
        }

        public async Task DeleteRegisteredObject(string messageValue)
        {
            var userId = JsonSerializer.Deserialize<string>(messageValue);

            // Fetch user from db
            var user = await _usersService.GetAsync(userId);

            if (user == null)
            {
                _producerService.Produce("coin-deleted", messageValue); // Return to queue
                throw new Exception("User not found.");
            }

            user.DeincrementRegisteredObjects();

            // Update user in db
            await _usersService.UpdateAsync(userId, user);
        }
    }
}

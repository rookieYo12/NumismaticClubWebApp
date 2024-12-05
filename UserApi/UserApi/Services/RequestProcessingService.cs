using UserApi.Models;
using UserApi.Services;
using System.Text.Json;

namespace UserApi.Services
{
    // Service process Kafka messages
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

        public async Task ProcessRequest(string message)
        {
            var requestObj = JsonSerializer.Deserialize<Request>(message);

            // Fetch user from db
            var user = await _usersService.GetAsync(requestObj.UserId);

            user.RegisteredObjects++;

            // Update user in db
            await _usersService.UpdateAsync(requestObj.UserId, user);
            
            // Get update time
            DateTimeOffset now = DateTimeOffset.Now;
            string updateTime = now.ToString();

            var response = JsonSerializer.Serialize(new Response
            {
                CoinId = requestObj.CoinId,
                UpdateTime = updateTime
            });

            _producerService.Produce(response);
        }
    }
}

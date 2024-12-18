using NumismaticClub.Models;
using NumismaticClub.Services;
using System.Text.Json;

namespace NumismaticClub.Services
{
    // Service processes Kafka messages
    public class MessageProcessingService
    {
        private readonly CoinsService _coinsService;

        public MessageProcessingService(CoinsService coinsService)
        {
            _coinsService = coinsService;
        }

        public async Task Process(string message)
        {
            var response = JsonSerializer.Deserialize<CoinConfirmedResponse>(message);

            // Fetch coin from db
            var coin = await _coinsService.GetAsync(response.CoinId);

            // Set date
            coin.setConfirmed(response.UpdateTime);

            // Update coin in db
            await _coinsService.UpdateAsync(response.CoinId, coin);
        }
    }
}

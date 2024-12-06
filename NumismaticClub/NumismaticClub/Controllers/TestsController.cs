using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using NumismaticClub.Models;
using NumismaticClub.Services;
using System.ComponentModel.DataAnnotations;
using System.Net.WebSockets;
using System.Text.Json;

namespace NumismaticClub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestsController : ControllerBase
    {
        private readonly CoinsService _coinsService;
        private readonly ProducerService _producer;

        public TestsController(CoinsService coinsService, ProducerService producer)
        {
            _coinsService = coinsService;
            _producer = producer;
        }

        // Генерация случайных данных для монет
        private List<Coin> GenerateRandomCoins(int count, string UserId)
        {
            var random = new Random();
            var coins = new List<Coin>();
            for (int i = 0; i < count; i++)
            {
                coins.Add(new Coin
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Year = random.Next(1800, 2024),
                    Country = $"Country {i}",
                    Value = random.Next(1, 1000),
                    UserId = UserId,
                });
            }
            return coins;
        }

        // Тест 1: Добавление 100 монет
        [HttpPost("add-100-coins")]
        public async Task<IActionResult> Add100Coins([Required] string UserId)
        {
            var coins = GenerateRandomCoins(100, UserId);

            foreach (var coin in coins)
            {
                await _coinsService.CreateAsync(coin); // Add to db

                Request request = new Request
                {
                    UserId = coin.UserId,
                    CoinId = coin.Id
                };

                // Send request to another service
                _producer.Produce(JsonSerializer.Serialize(request));
            }

            return Ok(new { message = "100 монет успешно добавлены в базу данных." });
        }

        // Тест 2: Добавление 100,000 монет
        [HttpPost("add-100000-coins")]
        public async Task<IActionResult> Add100000Coins(string UserId)
        {
            var coins = GenerateRandomCoins(100000, UserId);

            foreach (var coin in coins)
            {
                await _coinsService.CreateAsync(coin);

                Request request = new Request
                {
                    UserId = coin.UserId,
                    CoinId = coin.Id
                };

                // Send request to another service
                _producer.Produce(JsonSerializer.Serialize(request));
            }

            return Ok(new { message = "100 000 монет успешно добавлены в базу данных." });
        }

        // Тест 3: Удаление всех монет
        [HttpDelete("delete-all-coins")]
        public async Task<IActionResult> DeleteAllCoins()
        {
            var coins = await _coinsService.GetAsync();

            foreach (var coin in coins)
            {
                await _coinsService.RemoveAsync(coin.Id);
            }

            return Ok(new { message = "Все монеты успешно удалены." });
        }
    }
}

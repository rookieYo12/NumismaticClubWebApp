using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using NumismaticClub.Models;
using NumismaticClub.Services;

namespace NumismaticClub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestsController : ControllerBase
    {
        private readonly CoinsService _coinsService;

        public TestsController(CoinsService coinsService)
        {
            _coinsService = coinsService;
        }

        // Генерация случайных данных для монет
        private List<Coin> GenerateRandomCoins(int count)
        {
            var random = new Random();
            var coins = new List<Coin>();
            for (int i = 0; i < count; i++)
            {
                coins.Add(new Coin
                {
                    Id = ObjectId.GenerateNewId().ToString(),  // Генерация корректного ObjectId
                    Year = random.Next(1800, 2024),
                    Country = $"Country {i}",
                    Value = random.Next(1, 1000),
                });
            }
            return coins;
        }

        // Тест 1: Добавление 100 монет
        [HttpPost("add-100-coins")]
        public async Task<IActionResult> Add100Coins()
        {
            var coins = GenerateRandomCoins(100);

            foreach (var coin in coins)
            {
                await _coinsService.CreateAsync(coin);
            }

            return Ok(new { message = "100 монет успешно добавлены в базу данных." });
        }

        // Тест 2: Добавление 100,000 монет
        [HttpPost("add-100000-coins")]
        public async Task<IActionResult> Add100000Coins()
        {
            var coins = GenerateRandomCoins(100000);

            foreach (var coin in coins)
            {
                await _coinsService.CreateAsync(coin);
            }

            return Ok(new { message = "100,000 монет успешно добавлены в базу данных." });
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

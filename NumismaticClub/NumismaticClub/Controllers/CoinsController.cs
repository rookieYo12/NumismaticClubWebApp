using Microsoft.AspNetCore.Mvc;
using NumismaticClub.Services;
using NumismaticClub.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace NumismaticClub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoinsController : ControllerBase
    {
        private readonly CoinsService _coinsService;
        private readonly IDistributedCache _cache;
        private readonly ProducerService _producer;

        public CoinsController(CoinsService coinsService, IDistributedCache cache, ProducerService producer)
        {
            _coinsService = coinsService;
            _cache = cache;
            _producer = producer;
        }

        [HttpGet]
        public async Task<List<Coin>> Get() =>
            await _coinsService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Coin>> Get(string id)
        {
            Coin? coin;

            string cacheKey = $"Coin_{id}";
            var cachedCoin = await _cache.GetStringAsync(cacheKey); // Try get data from cache

            if (!string.IsNullOrEmpty(cachedCoin))
            {
                // If data in cache deserialize and return it
                coin = JsonSerializer.Deserialize<Coin>(cachedCoin) ?? new Coin();
                return Ok(new { Message = "Data fetch from cache.", Data = coin });
            }

            // Fetch data from db
            coin = await _coinsService.GetAsync(id);
                
            if (coin is null)
            {
                return NotFound();
            }

            var serializedCoin = JsonSerializer.Serialize(coin);

            // Put serialized data to cache
            await _cache.SetStringAsync(cacheKey, serializedCoin, new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(30)
            });

            return Ok(new { Message = "Data fetch from database.", Data = coin });
        }

        [HttpPost]
        public async Task<IActionResult> Post(Coin newCoin)
        {
            await _coinsService.CreateAsync(newCoin);

            Request request = new Request
            {
                UserId = newCoin.UserId,
                CoinId = newCoin.Id
            };

            _producer.Produce(JsonSerializer.Serialize(request));

            return CreatedAtAction(nameof(Get), new { id = newCoin.Id }, newCoin);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Coin updatedCoin)
        {
            if (id != updatedCoin.Id)
            {
                return BadRequest("Coin ID mismatch.");
            }

            var coin = await _coinsService.GetAsync(id);
            if (coin is null)
            {
                return NotFound();
            }
            
            // If find coin in db update it
            await _coinsService.UpdateAsync(id, updatedCoin);

            // Cache new data          
            string cacheKey = $"Coin_{id}";
            var serializedCoin = JsonSerializer.Serialize(updatedCoin);

            await _cache.SetStringAsync(cacheKey, serializedCoin, new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(30)
            });

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var coin = await _coinsService.GetAsync(id);
            if (coin is null)
            {
                return NotFound();
            }

            await _coinsService.RemoveAsync(id); // Delete data from db

            // Remove data from cache
            var cacheKey = $"Coin_{id}";
            await _cache.RemoveAsync(cacheKey);

            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using NumismaticClub.Services;
using NumismaticClub.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
            var userId = Request.Headers["UserId"].ToString(); // Get user id from header

            newCoin.setUserId(userId);
            
            await _coinsService.CreateAsync(newCoin);

            CoinCreatedRequest request = new CoinCreatedRequest
            {
                UserId = newCoin.UserId,
                CoinId = newCoin.Id
            };

            // Send request to user service for data update 
            _producer.Produce("coin-created", JsonSerializer.Serialize(request));

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

            var roles = Request.Headers["Roles"].ToString().Split(',');
            
            if (!roles.Contains("Admin"))
            {
                // Сheck that the owner is trying to update
                var userId = Request.Headers["UserId"].ToString();

                if (userId != coin.UserId)
                {
                    return BadRequest("User with the role 'User' cannot modify objects of other users.");
                }
            }

            // Copy some data from old coin
            updatedCoin.setUserId(coin.UserId);
            updatedCoin.setConfirmed(coin.Confirmed);

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

            var roles = Request.Headers["Roles"].ToString().Split(',');

            if (!roles.Contains("Admin"))
            {
                // Сheck that the owner is trying to delete
                var userId = Request.Headers["UserId"].ToString();

                if (userId != coin.UserId)
                {
                    return BadRequest("User with the role 'User' cannot delete objects of other users.");
                }
            }

            var coinOwnerId = coin.UserId;

            await _coinsService.RemoveAsync(id); // Delete data from db

            // Remove data from cache
            var cacheKey = $"Coin_{id}";
            await _cache.RemoveAsync(cacheKey);

            _producer.Produce("coin-deleted", JsonSerializer.Serialize(coinOwnerId));

            return NoContent();
        }
    }
}

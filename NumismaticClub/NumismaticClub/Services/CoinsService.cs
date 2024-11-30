using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NumismaticClub.Models;

namespace NumismaticClub.Services
{
    public class CoinsService
    {
        private readonly IMongoCollection<Coin> _coinsCollection;

        // Get collection and put into _coinsCollection
        public CoinsService(
            IOptions<CoinsDatabaseSettings> coinsDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                coinsDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                coinsDatabaseSettings.Value.DatabaseName);

            _coinsCollection = mongoDatabase.GetCollection<Coin>(
                coinsDatabaseSettings.Value.CoinsCollectionName);
        }

        // Get all coins
        public async Task<List<Coin>> GetAsync() =>
            await _coinsCollection.Find(_ => true).ToListAsync();

        // Get coin by id
        public async Task<Coin?> GetAsync(string id) =>
            await _coinsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        // Insert a new coin
        public async Task CreateAsync(Coin newCoin) =>
            await _coinsCollection.InsertOneAsync(newCoin);

        // Update coin by id (replace)
        public async Task UpdateAsync(string id, Coin updatedBook) =>
            await _coinsCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        // Delete coin
        public async Task RemoveAsync(string id) =>
            await _coinsCollection.DeleteOneAsync(x => x.Id == id);
    }
}

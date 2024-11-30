using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UserApi.Models;

namespace UserApi.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<User> _usersCollection;

        // Get collection and put into _usersCollection
        public UsersService(
            IOptions<UsersDbSettings> usersDbSettings)
        {
            var mongoClient = new MongoClient(
                usersDbSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                usersDbSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<User>(
                usersDbSettings.Value.UsersCollectionName);
        }

        // Get all
        public async Task<List<User>> GetAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        // Get by id
        public async Task<User?> GetAsync(string id) =>
            await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        // Insert a new
        public async Task CreateAsync(User newUser) =>
            await _usersCollection.InsertOneAsync(newUser);

        // Update by id (replace)
        public async Task UpdateAsync(string id, User updatedUser) =>
            await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

        // Delete
        public async Task RemoveAsync(string id) =>
            await _usersCollection.DeleteOneAsync(x => x.Id == id);
    }
}


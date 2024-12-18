using MongoDB.Driver;
using AuthApi.Models;
using Microsoft.Extensions.Options;

namespace AuthApi.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _usersCollection;

        // Get users collection from db
        public UserService(IOptions<DbSettings> dbSettings)
        {
            var mongoClient = new MongoClient(
                dbSettings.Value.ConnectionString);

            var mongoDb = mongoClient.GetDatabase(
                dbSettings.Value.DatabaseName);

            _usersCollection = mongoDb.GetCollection<User>(
                dbSettings.Value.UsersCollectionName);
        }

        // Get user by login
        public async Task<User?> GetByLoginAsync(string login) =>
            await _usersCollection.Find(x => x.Login == login).FirstOrDefaultAsync();

        // Get user by id
        public async Task<User?> GetByIdAsync(string id) =>
            await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        // Insert a new user
        public async Task CreateAsync(User newUser) =>
            await _usersCollection.InsertOneAsync(newUser);

        // Update by login
        public async Task UpdateAsync(string login, User updatedUser) =>
            await _usersCollection.ReplaceOneAsync(x => x.Login == login, updatedUser);

        // Delete by id
        public async Task DeleteAsync(string id) =>
            await _usersCollection.DeleteOneAsync(x => x.Id == id);
    }
}

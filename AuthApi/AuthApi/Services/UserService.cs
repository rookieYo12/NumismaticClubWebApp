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

        // Get user by username
        public async Task<User?> GetAsync(string name) =>
            await _usersCollection.Find(x => x.Name == name).FirstOrDefaultAsync();

        // Insert a new user
        public async Task CreateAsync(User newUser) =>
            await _usersCollection.InsertOneAsync(newUser);
    }
}

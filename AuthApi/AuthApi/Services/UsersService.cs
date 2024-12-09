using MongoDB.Driver;
using AuthApi.Models;
using Microsoft.Extensions.Options;

namespace AuthApi.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<User> _usersCollection;

        // Get users collection from db
        public UsersService(IOptions<DbSettings> dbSettings)
        {
            var mongoClient = new MongoClient(
                dbSettings.Value.ConnectionString);

            var mongoDb = mongoClient.GetDatabase(
                dbSettings.Value.DatabaseName);

            _usersCollection = mongoDb.GetCollection<User>(
                dbSettings.Value.UsersCollectionName);
        }

        // Get user by login
        public async Task<User?> GetAsync(string login) =>
            await _usersCollection.Find(x => x.Login == login).FirstOrDefaultAsync();

        // Insert a new user
        public async Task CreateAsync(User newUser) =>
            await _usersCollection.InsertOneAsync(newUser);
    }
}

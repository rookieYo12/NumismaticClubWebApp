using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AuthApi.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Password { get; set; } = null!;

        public UserRole Role { get; set; }

        public string RefreshToken { get; set; } = null!;

        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}

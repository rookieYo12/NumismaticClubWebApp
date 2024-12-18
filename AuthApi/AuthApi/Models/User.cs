﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AuthApi.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;

        public List<UserRole> Roles { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}

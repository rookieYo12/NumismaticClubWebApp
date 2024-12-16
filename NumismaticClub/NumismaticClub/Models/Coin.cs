using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NumismaticClub.Models
{
    public class Coin
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int Year { get; set; }

        public string Country { get; set; } = null!;

        public int Value { get; set; }

        public string? UserId { get; private set; }

        public string Confirmed { get; private set; } = "Not confirmed.";

        public void setConfirmed(string value)
        {
            Confirmed = value;
        }

        public void setUserId(string value)
        {
            UserId = value;
        }
    }
}

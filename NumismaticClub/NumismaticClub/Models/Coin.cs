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

        public string UserId { get; set; } = null!;

        public string Confirmed { get; private set; } = "Not confirmed.";

        public void setConfirmed(string value)
        {
            Confirmed = value;
        }
    }
}

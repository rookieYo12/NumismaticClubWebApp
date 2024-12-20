using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UserApi.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } = null!;

        public string Surname { get; set; } = null!;

        public int RegisteredObjects { get; private set; } = 0;

        public void IncremetRegisteredObjects()
        {
            RegisteredObjects++;
        }

        public void DeincrementRegisteredObjects()
        {
            RegisteredObjects--;
        }
    }
}

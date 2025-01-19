using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OrdersMS.src.Orders.Infrastructure.Models
{
    public class MongoExtraCost
    {
        [BsonId]
        [BsonElement("id"), BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }

        [BsonElement("orderId")]
        public required string OrderId { get; set; }

        [BsonElement("name")]
        public required string Name { get; set; }

        [BsonElement("price")]
        public required decimal Price { get; set; }
    }
}

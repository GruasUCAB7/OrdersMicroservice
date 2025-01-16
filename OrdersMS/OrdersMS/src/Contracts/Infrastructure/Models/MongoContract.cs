using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OrdersMS.src.Contracts.Infrastructure.Models
{
    public class MongoContract
    {
        [BsonId]
        [BsonElement("id"), BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }

        [BsonElement("contractNumber")]
        public required int ContractNumber { get; set; }

        [BsonElement("associatedPolicy")]
        public required string AssociatedPolicy { get; set; }

        [BsonElement("insuredVehicle")]
        public required string InsuredVehicle { get; set; }

        [BsonElement("startDate")]
        public required DateTime StartDate { get; set; }

        [BsonElement("expirationDate")]
        public required DateTime ExpirationDate { get; set; }

        [BsonElement("status")]
        public required string Status { get; set; }

        [BsonElement("createdDate")]
        public required DateTime CreatedDate { get; set; }

        [BsonElement("updatedDate")]
        public required DateTime UpdatedDate { get; set; }
    }
}

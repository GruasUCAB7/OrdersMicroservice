using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OrdersMS.src.InsuredVehicles.Infrastructure.Models
{
    public class MongoInsuredVehicle
    {
        [BsonId]
        [BsonElement("id"), BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }

        [BsonElement("brand")]
        public required string Brand { get; set; }

        [BsonElement("model")]
        public required string Model { get; set; }

        [BsonElement("plate")]
        public required string Plate { get; set; }

        [BsonElement("vehicleSize")]
        public required string VehicleSize { get; set; }

        [BsonElement("year")]
        public required int Year { get; set; }

        [BsonElement("isActive")]
        public required bool IsActive { get; set; }

        [BsonElement("clientDNI")]
        public required string ClientDNI { get; set; }

        [BsonElement("clientName")]
        public required string ClientName { get; set; }

        [BsonElement("clientEmail")]
        public required string ClientEmail { get; set; }

        [BsonElement("createdDate")]
        public required DateTime CreatedDate { get; set; }

        [BsonElement("updatedDate")]
        public required DateTime UpdatedDate { get; set; }
    }
}

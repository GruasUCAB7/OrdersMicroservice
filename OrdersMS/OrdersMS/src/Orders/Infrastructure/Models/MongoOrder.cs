using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OrdersMS.src.Orders.Infrastructure.Models
{
    public class MongoOrder
    {
        [BsonId]
        [BsonElement("id"), BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }

        [BsonElement("contractClient")]
        public required string ContractClient { get; set; }

        [BsonElement("createdByOperator")]
        public required string CreatedByOperator { get; set; }

        [BsonElement("driverAssigned")]
        public required string DriverAssigned { get; set; }

        [BsonElement("incidentAddress")]
        public required MongoCoordinates IncidentAddress { get; set; }

        [BsonElement("destinationAddress")]
        public required MongoCoordinates DestinationAddress { get; set; }

        [BsonElement("incidentType")]
        public required string IncidentType { get; set; }

        [BsonElement("incidentDate")]
        public required string IncidentDate { get; set; }

        [BsonElement("extraServicesApplied")]
        public required List<MongoExtraServicesApplied> ExtraServicesApplied { get; set; }

        [BsonElement("totalCost")]
        public decimal TotalCost { get; set; }

        [BsonElement("status")]
        public required string Status { get; set; }

        [BsonElement("createdDate")]
        public required DateTime CreatedDate { get; set; }

        [BsonElement("updatedDate")]
        public required DateTime UpdatedDate { get; set; }
    }

    public class MongoCoordinates
    {
        [BsonElement("latitude")]
        public required double Latitude { get; set; }

        [BsonElement("longitude")]
        public required double Longitude { get; set; }
    }

    public class MongoExtraServicesApplied
    {

        [BsonElement("extraCostId")]
        public required string Id { get; set; }

        [BsonElement("name")]
        public required string Name { get; set; }

        [BsonElement("price")]
        public required decimal Price { get; set; }
    }
}

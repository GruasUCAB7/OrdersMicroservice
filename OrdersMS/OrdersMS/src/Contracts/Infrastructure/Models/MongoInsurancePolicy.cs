using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OrdersMS.src.Contracts.Infrastructure.Models
{
    public class MongoInsurancePolicy
    {
        [BsonId]
        [BsonElement("id"), BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }

        [BsonElement("policyType")]
        public required string PolicyType { get; set; }

        [BsonElement("coverageKm")]
        public required int CoverageKm { get; set; }

        [BsonElement("coverageAmount")]
        public required double CoverageAmount { get; set; }

        [BsonElement("priceExtraKm")]
        public required double PriceExtraKm { get; set; }

        [BsonElement("isActive")]
        public required bool IsActive { get; set; }
    }
}

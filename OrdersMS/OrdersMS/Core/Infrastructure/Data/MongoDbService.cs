using MongoDB.Bson;
using MongoDB.Driver;

namespace OrdersMS.Core.Infrastructure.Data
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _databaseGruas;
        private readonly IMongoDatabase _databaseMassTransit;

        public MongoDbService()
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            var databaseNameGruas = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME_GRUAS_UCAB");
            var databaseNameMassTransit = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME_MASSTRANSIT");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "MongoDB connection string cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(databaseNameGruas))
            {
                throw new ArgumentNullException(nameof(databaseNameGruas), "MongoDB database name cannot be null or empty.");
            }

            var mongoClient = new MongoClient(connectionString);
            _databaseGruas = mongoClient.GetDatabase(databaseNameGruas);
            _databaseMassTransit = mongoClient.GetDatabase(databaseNameMassTransit);
        }

        public IMongoCollection<BsonDocument> GetOrderCollection()
        {
            return _databaseGruas.GetCollection<BsonDocument>("order");
        }

        public IMongoCollection<BsonDocument> GetInsuredVehicleCollection()
        {
            return _databaseGruas.GetCollection<BsonDocument>("insuredVehicle");
        }

        public IMongoCollection<BsonDocument> GetInsurancePolicyCollection()
        {
            return _databaseGruas.GetCollection<BsonDocument>("insurancePolicy");
        }

        public IMongoCollection<BsonDocument> GetContractCollection()
        {
            return _databaseGruas.GetCollection<BsonDocument>("contract");
        }

        public IMongoCollection<BsonDocument> GetExtraCostCollection()
        {
            return _databaseGruas.GetCollection<BsonDocument>("extraCost");
        }

        public IMongoCollection<BsonDocument> GeMassTransitCollection()
        {
            return _databaseMassTransit.GetCollection<BsonDocument>("massTransit");
        }
    }
}

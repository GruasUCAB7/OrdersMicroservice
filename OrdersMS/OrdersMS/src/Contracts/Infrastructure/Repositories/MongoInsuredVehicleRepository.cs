using MongoDB.Bson;
using MongoDB.Driver;
using OrdersMS.Core.Infrastructure.Data;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Queries.GetAllVehicles.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Contracts.Infrastructure.Models;

namespace OrdersMS.src.Contracts.Infrastructure.Repositories
{
    public class MongoInsuredVehicleRepository(MongoDbService mongoDbService) : IInsuredVehicleRepository
    {
        private readonly IMongoCollection<BsonDocument> _vehicleCollection = mongoDbService.GetInsuredVehicleCollection();

        public async Task<bool> ExistByPlate(string plate)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("plate", plate);
            var vehicle = await _vehicleCollection.Find(filter).FirstOrDefaultAsync();
            return vehicle != null;
        }

        public async Task<List<InsuredVehicle>> GetAll(GetAllVehiclesQuery data)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = data.IsActive?.ToLower() switch
            {
                "active" => filterBuilder.Eq("isActive", true),
                "inactive" => filterBuilder.Eq("isActive", false),
                _ => filterBuilder.Empty
            };

            var vehicleEntities = await _vehicleCollection
                .Find(filter)
                .Skip(data.PerPage * (data.Page - 1))
                .Limit(data.PerPage)
                .ToListAsync();

            var vehicles = vehicleEntities.Select(v =>
            {
                var vehicle = new InsuredVehicle(
                    new VehicleId(v.GetValue("_id").AsString),
                    new VehicleBrand(v.GetValue("brand").AsString),
                    new VehicleModel(v.GetValue("model").AsString),
                    new VehiclePlate(v.GetValue("plate").AsString),
                    new VehicleSize(v.GetValue("vehicleSize").AsString),
                    new VehicleYear(v.GetValue("year").AsInt32),
                    new ClientDNI(v.GetValue("clientDNI").AsString),
                    new ClientName(v.GetValue("clientName").AsString),
                    new ClientEmail(v.GetValue("clientEmail").AsString)
                );

                vehicle.SetIsActive(v.GetValue("isActive").AsBoolean);
                return vehicle;
            }).ToList();

            return vehicles;
        }

        public async Task<Core.Utils.Optional.Optional<InsuredVehicle>> GetById(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var vehicleDocument = await _vehicleCollection.Find(filter).FirstOrDefaultAsync();

            if (vehicleDocument is null)
            {
                return Core.Utils.Optional.Optional<InsuredVehicle>.Empty();
            }

            var vehicle = new InsuredVehicle(
                new VehicleId(vehicleDocument.GetValue("_id").AsString),
                new VehicleBrand(vehicleDocument.GetValue("brand").AsString),
                new VehicleModel(vehicleDocument.GetValue("model").AsString),
                new VehiclePlate(vehicleDocument.GetValue("plate").AsString),
                new VehicleSize(vehicleDocument.GetValue("vehicleSize").AsString),
                new VehicleYear(vehicleDocument.GetValue("year").AsInt32),
                new ClientDNI(vehicleDocument.GetValue("clientDNI").AsString),
                new ClientName(vehicleDocument.GetValue("clientName").AsString),
                new ClientEmail(vehicleDocument.GetValue("clientEmail").AsString)
            );

            vehicle.SetIsActive(vehicleDocument.GetValue("isActive").AsBoolean);

            return Core.Utils.Optional.Optional<InsuredVehicle>.Of(vehicle);
        }

        public async Task<Result<InsuredVehicle>> Save(InsuredVehicle vehicle)
        {
            var mongoVehicle = new MongoInsuredVehicle
            {
                Id = vehicle.GetId(),
                Brand = vehicle.GetBrand(),
                Model = vehicle.GetModel(),
                Plate = vehicle.GetPlate(),
                VehicleSize = vehicle.GetVehicleSize(),
                Year = vehicle.GetYear(),
                IsActive = vehicle.GetIsActive(),
                ClientDNI = vehicle.GetClientDNI(),
                ClientName = vehicle.GetClientName(),
                ClientEmail = vehicle.GetClientEmail(),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var bsonDocument = new BsonDocument
            {
                {"_id", mongoVehicle.Id},
                {"brand", mongoVehicle.Brand},
                {"model", mongoVehicle.Model},
                {"plate", mongoVehicle.Plate},
                {"vehicleSize", mongoVehicle.VehicleSize},
                {"year", mongoVehicle.Year},
                {"isActive", mongoVehicle.IsActive},
                {"clientDNI", mongoVehicle.ClientDNI},
                {"clientName", mongoVehicle.ClientName},
                {"clientEmail", mongoVehicle.ClientEmail},
                {"createdDate", mongoVehicle.CreatedDate},
                {"updatedDate", mongoVehicle.UpdatedDate}
            };

            await _vehicleCollection.InsertOneAsync(bsonDocument);

            var savedVehicle = new InsuredVehicle(
                new VehicleId(mongoVehicle.Id),
                new VehicleBrand(mongoVehicle.Brand),
                new VehicleModel(mongoVehicle.Model),
                new VehiclePlate(mongoVehicle.Plate),
                new VehicleSize(mongoVehicle.VehicleSize),
                new VehicleYear(mongoVehicle.Year),
                new ClientDNI(mongoVehicle.ClientDNI),
                new ClientName(mongoVehicle.ClientName),
                new ClientEmail(mongoVehicle.ClientEmail)
            );

            return Result<InsuredVehicle>.Success(savedVehicle);
        }

        public async Task<Result<InsuredVehicle>> Update(InsuredVehicle vehicle)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", vehicle.GetId());
            var update = Builders<BsonDocument>.Update
                .Set("isActive", vehicle.GetIsActive());

            var updateResult = await _vehicleCollection.UpdateOneAsync(filter, update);

            if (updateResult.ModifiedCount == 0)
            {
                return Result<InsuredVehicle>.Failure(new Exception("Failed to update vehicle"));
            }

            return Result<InsuredVehicle>.Success(vehicle);
        }

        public async Task<bool> IsActiveVehicle(string id)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("_id", id),
                Builders<BsonDocument>.Filter.Eq("isActive", true)
            );

            var vehicle = await _vehicleCollection.Find(filter).FirstOrDefaultAsync();
            return vehicle != null;
        }
    }
}

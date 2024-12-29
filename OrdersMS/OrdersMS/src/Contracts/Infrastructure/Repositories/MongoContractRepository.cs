using MongoDB.Bson;
using MongoDB.Driver;
using OrdersMS.Core.Infrastructure.Data;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Queries.GetAllContracts.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Contracts.Infrastructure.Models;

namespace OrdersMS.src.Contracts.Infrastructure.Repositories
{
    public class MongoContractRepository(MongoDbService mongoDbService) : IContractRepository
    {
        private readonly IMongoCollection<BsonDocument> _contractCollection = mongoDbService.GetContractCollection();


        public async Task<List<Contract>> GetAll(GetAllContractsQuery data)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = data.Status?.ToLower() switch
            {
                "activo" => filterBuilder.Eq("status", "Activo"),
                "expirado" => filterBuilder.Eq("status", "Expirado"),
                "cancelado" => filterBuilder.Eq("status", "Cancelado"),
                _ => filterBuilder.Empty
            };

            var contractEntities = await _contractCollection
                .Find(filter)
                .Skip(data.PerPage * (data.Page - 1))
                .Limit(data.PerPage)
                .ToListAsync();

            var contracts = contractEntities.Select(c =>
            {
                var contract = Contract.CreateContract
                (
                    new ContractId(c.GetValue("_id").AsString),
                    new ContractNumber(c.GetValue("contractNumber").AsInt32),
                    new PolicyId(c.GetValue("associatedPolicy").AsString),
                    new VehicleId(c.GetValue("insuredVehicle").AsString)
                );

                return contract;
            }).ToList();

            return contracts;
        }

        public async Task<Core.Utils.Optional.Optional<Contract>> GetById(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var contractDocument = await _contractCollection.Find(filter).FirstOrDefaultAsync();

            if (contractDocument is null)
            {
                return Core.Utils.Optional.Optional<Contract>.Empty();
            }

            var contract = Contract.CreateContract(
                new ContractId(contractDocument.GetValue("_id").AsString),
                new ContractNumber(contractDocument.GetValue("contractNumber").AsInt32),
                new PolicyId(contractDocument.GetValue("associatedPolicy").AsString),
                new VehicleId(contractDocument.GetValue("insuredVehicle").AsString)
            );

            return Core.Utils.Optional.Optional<Contract>.Of(contract);
        }

        public async Task<Result<Contract>> Save(Contract contract)
        {
            var mongoContract = new MongoContract
            {
                Id = contract.GetId(),
                ContractNumber = contract.GetContractNumber(),
                AssociatedPolicy= contract.GetPolicyId(),
                InsuredVehicle = contract.GetVehicleId(),
                StartDate = contract.GetStartDate(),
                ExpirationDate = contract.GetExpirationDate(),
                Status = contract.GetStatus(),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var bsonDocument = new BsonDocument
            {
                {"_id", mongoContract.Id},
                {"contractNumber", mongoContract.ContractNumber},
                {"associatedPolicy", mongoContract.AssociatedPolicy},
                {"insuredVehicle", mongoContract.InsuredVehicle},
                {"startDate", mongoContract.StartDate},
                {"expirationDate", mongoContract.ExpirationDate},
                {"status", mongoContract.Status},
                {"createdDate", mongoContract.CreatedDate},
                {"updatedDate", mongoContract.UpdatedDate}
            };

            await _contractCollection.InsertOneAsync(bsonDocument);

            var savedContract = Contract.CreateContract(
                new ContractId(mongoContract.Id),
                new ContractNumber(mongoContract.ContractNumber),
                new PolicyId(mongoContract.AssociatedPolicy),
                new VehicleId(mongoContract.InsuredVehicle)
            );

            return Result<Contract>.Success(savedContract);
        }

        public async Task<Result<Contract>> Update(Contract contract)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", contract.GetId());
            var update = Builders<BsonDocument>.Update
                .Set("status", contract.GetStatus());

            var updateResult = await _contractCollection.UpdateOneAsync(filter, update);

            if (updateResult.ModifiedCount == 0)
            {
                return Result<Contract>.Failure(new Exception("Failed to update contract"));
            }

            return Result<Contract>.Success(contract);
        }

        public async Task<bool> ContractExists(string policyId, string vehicleId)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("associatedPolicy", policyId),
                Builders<BsonDocument>.Filter.Eq("insuredVehicle", vehicleId)
            );

            var contract = await _contractCollection.Find(filter).FirstOrDefaultAsync();
            return contract != null;
        }

        public async Task<bool> IsActiveContract(string id)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("_id", id),
                Builders<BsonDocument>.Filter.Eq("status", "Activo")
            );

            var contract = await _contractCollection.Find(filter).FirstOrDefaultAsync();
            return contract != null;
        }

        public async Task<bool> IsContractNumberExists(int contractNumber)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("contractNumber", contractNumber));

            var numberExist = await _contractCollection.Find(filter).FirstOrDefaultAsync();
            return numberExist != null;
        }
    }
}

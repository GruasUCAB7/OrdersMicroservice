using MongoDB.Bson;
using MongoDB.Driver;
using OrdersMS.Core.Infrastructure.Data;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Queries.GetAllExtraCosts.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;
using OrdersMS.src.Orders.Infrastructure.Models;

namespace OrdersMS.src.Orders.Infrastructure.Repositories
{
    public class MongoExtraCostRepository(MongoDbService mongoDbService) : IExtraCostRepository
    {
        private readonly IMongoCollection<BsonDocument> _extraCostCollection = mongoDbService.GetExtraCostCollection();

        public async Task<bool> ExistByName(string name)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("name", name);
            var extraCost = await _extraCostCollection.Find(filter).FirstOrDefaultAsync();
            return extraCost != null;
        }

        public async Task<List<ExtraCost>> GetAll(GetAllExtraCostsQuery data)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = data.IsActive?.ToLower() switch
            {
                "active" => filterBuilder.Eq("isActive", true),
                "inactive" => filterBuilder.Eq("isActive", false),
                _ => filterBuilder.Empty
            };

            var extraCostEntities = await _extraCostCollection
                .Find(filter)
                .Skip(data.PerPage * (data.Page - 1))
                .Limit(data.PerPage)
                .ToListAsync();

            var extraCosts = extraCostEntities.Select(e =>
            {
                var extraCost = new ExtraCost
                (
                    new ExtraCostId(e.GetValue("_id").AsString),
                    new ExtraCostName(e.GetValue("name").AsString)
                );

                extraCost.SetIsActive(e.GetValue("isActive").AsBoolean);
                return extraCost;
            }).ToList();

            return extraCosts;
        }

        public async Task<Core.Utils.Optional.Optional<ExtraCost>> GetById(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var extraCostDocument = await _extraCostCollection.Find(filter).FirstOrDefaultAsync();

            if (extraCostDocument is null)
            {
                return Core.Utils.Optional.Optional<ExtraCost>.Empty();
            }

            var extraCost = new ExtraCost(
                new ExtraCostId(extraCostDocument.GetValue("_id").AsString),
                new ExtraCostName(extraCostDocument.GetValue("name").AsString)
            );

            extraCost.SetIsActive(extraCostDocument.GetValue("isActive").AsBoolean);

            return Core.Utils.Optional.Optional<ExtraCost>.Of(extraCost);
        }

        public async Task<Result<ExtraCost>> Save(ExtraCost extraCost)
        {
            var mongoExtraCost = new MongoExtraCost
            {
                Id = extraCost.GetId(),
                Name = extraCost.GetName(),
                IsActive = extraCost.GetIsActive(),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var bsonDocument = new BsonDocument
            {
                {"_id", mongoExtraCost.Id},
                {"name", mongoExtraCost.Name},
                {"isActive", mongoExtraCost.IsActive},
                {"createdDate", mongoExtraCost.CreatedDate},
                {"updatedDate", mongoExtraCost.UpdatedDate}
            };

            await _extraCostCollection.InsertOneAsync(bsonDocument);

            var savedExtraCost = new ExtraCost(
                new ExtraCostId(mongoExtraCost.Id),
                new ExtraCostName(mongoExtraCost.Name)
            );

            return Result<ExtraCost>.Success(savedExtraCost);
        }

        public async Task<Result<ExtraCost>> Update(ExtraCost extraCost)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", extraCost.GetId());
            var update = Builders<BsonDocument>.Update
                .Set("isActive", extraCost.GetIsActive());

            var updateResult = await _extraCostCollection.UpdateOneAsync(filter, update);

            if (updateResult.ModifiedCount == 0)
            {
                return Result<ExtraCost>.Failure(new Exception("Failed to update extra cost"));
            }

            return Result<ExtraCost>.Success(extraCost);
        }

        public async Task<bool> IsActiveExtraCost(string id)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("_id", id),
                Builders<BsonDocument>.Filter.Eq("isActive", true)
            );

            var extraCost = await _extraCostCollection.Find(filter).FirstOrDefaultAsync();
            return extraCost != null;
        }
    }
}

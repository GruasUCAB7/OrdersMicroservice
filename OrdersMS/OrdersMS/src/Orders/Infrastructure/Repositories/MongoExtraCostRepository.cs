using MongoDB.Bson;
using MongoDB.Driver;
using OrdersMS.Core.Infrastructure.Data;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;
using OrdersMS.src.Orders.Infrastructure.Models;

namespace OrdersMS.src.Orders.Infrastructure.Repositories
{
    public class MongoExtraCostRepository(MongoDbService mongoDbService) : IExtraCostRepository
    {
        private readonly IMongoCollection<BsonDocument> _extraCostCollection = mongoDbService.GetExtraCostCollection();

        public async Task<List<ExtraCost>> GetExtraCostByOrderId(string orderId)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("orderId", orderId);

            var extraCostEntities = await _extraCostCollection.Find(filter).ToListAsync();

            var extraCosts = extraCostEntities.Select(e =>
            {
                var extraCost = new ExtraCost(
                    new ExtraCostId(e.GetValue("_id").AsString),
                    new OrderId(e.GetValue("orderId").AsString),
                    new ExtraCostName(e.GetValue("name").AsString),
                    new ExtraCostPrice(e.GetValue("price").AsDecimal)
                );
                return extraCost;
            }).ToList();

            return extraCosts;
        }

        public async Task<Result<ExtraCost>> Save(ExtraCost extraCost)
        {
            var mongoExtraCost = new MongoExtraCost
            {
                Id = extraCost.GetId(),
                OrderId = extraCost.GetOrderId(),
                Name = extraCost.GetName(),
                Price = extraCost.GetPrice()
            };

            var bsonDocument = new BsonDocument
            {
                {"_id", mongoExtraCost.Id},
                {"orderId", mongoExtraCost.OrderId},
                {"name", mongoExtraCost.Name},
                {"price", mongoExtraCost.Price}
            };

            await _extraCostCollection.InsertOneAsync(bsonDocument);

            var savedExtraCost = new ExtraCost(
                    new ExtraCostId(mongoExtraCost.Id),
                    new OrderId(mongoExtraCost.OrderId),
                    new ExtraCostName(mongoExtraCost.Name),
                    new ExtraCostPrice(mongoExtraCost.Price)
                );
            return Result<ExtraCost>.Success(savedExtraCost);
        }
    }
}




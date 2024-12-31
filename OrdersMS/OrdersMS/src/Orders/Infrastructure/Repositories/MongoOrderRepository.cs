﻿using MongoDB.Bson;
using MongoDB.Driver;
using OrdersMS.Core.Infrastructure.Data;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;
using OrdersMS.src.Orders.Infrastructure.Models;

namespace OrdersMS.src.Orders.Infrastructure.Repositories
{
    public class MongoOrderRepository(MongoDbService mongoDbService) : IOrderRepository
    {
        private readonly IMongoCollection<BsonDocument> _orderCollection = mongoDbService.GetOrderCollection();

        public async Task<List<Order>> GetAll(GetAllOrdersQuery data)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter;

            var validStatuses = new List<string>
            {
                "por asignar",
                "por aceptar",
                "aceptado",
                "localizado",
                "en proceso",
                "finalizado",
                "cancelada",
                "pagado"
            };

            if (!string.IsNullOrEmpty(data.Status) && validStatuses.Contains(data.Status.ToLower()))
            {
                filter = filterBuilder.Eq("status", data.Status);
            }
            else
            {
                filter = filterBuilder.Empty;
            }

            var orderEntities = await _orderCollection
                .Find(filter)
                .Skip(data.PerPage * (data.Page - 1))
                .Limit(data.PerPage)
                .ToListAsync();

            var orders = orderEntities.Select(o =>
            {
                var extraServicesApplied = o.GetValue("extraServicesApplied").AsBsonArray
                .Select(extraService => new ExtraCost(
                    new ExtraCostId(extraService["id"].AsString),
                    new ExtraCostName(extraService["name"].AsString),
                    new ExtraCostPrice(extraService["price"].AsDecimal)
                )).ToList();

                var order = Order.CreateOrder
                (
                    new OrderId(o.GetValue("_id").AsString),
                    new ContractId(o.GetValue("contractClient").AsString),
                    new DriverId(o.GetValue("driverAssigned").AsString),
                    new Coordinates(
                        o.GetValue("incidentAddress").AsBsonDocument.GetValue("latitude").AsDouble,
                        o.GetValue("incidentAddress").AsBsonDocument.GetValue("longitude").AsDouble
                    ),
                    new Coordinates(
                        o.GetValue("destinationAddress").AsBsonDocument.GetValue("latitude").AsDouble,
                        o.GetValue("destinationAddress").AsBsonDocument.GetValue("longitude").AsDouble
                    ),
                    extraServicesApplied
                );
                order.SetStatus(new OrderStatus(o.GetValue("status").AsString));
                order.SetExtraServicesApplied(extraServicesApplied);

                return order;
            }).ToList();

            return orders;
        }

        public async Task<Core.Utils.Optional.Optional<Order>> GetById(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var orderDocument = await _orderCollection.Find(filter).FirstOrDefaultAsync();

            if (orderDocument is null)
            {
                return Core.Utils.Optional.Optional<Order>.Empty();
            }

            var extraServicesApplied = orderDocument["extraServicesApplied"].AsBsonArray
                .Select(extraService => new ExtraCost(
                    new ExtraCostId(extraService["id"].AsString),
                    new ExtraCostName(extraService["name"].AsString),
                    new ExtraCostPrice(extraService["price"].AsDecimal)
                )).ToList();

            var order = Order.CreateOrder(
                new OrderId(orderDocument.GetValue("_id").AsString),
                new ContractId(orderDocument.GetValue("contractClient").AsString),
                new DriverId(orderDocument.GetValue("driverAssigned").AsString),
                new Coordinates(
                    orderDocument.GetValue("incidentAddress").AsBsonDocument.GetValue("latitude").AsDouble,
                    orderDocument.GetValue("incidentAddress").AsBsonDocument.GetValue("longitude").AsDouble
                ),
                new Coordinates(
                    orderDocument.GetValue("destinationAddress").AsBsonDocument.GetValue("latitude").AsDouble,
                    orderDocument.GetValue("destinationAddress").AsBsonDocument.GetValue("longitude").AsDouble
                ),
                extraServicesApplied
            );
            order.SetStatus(new OrderStatus(orderDocument.GetValue("status").AsString));
            order.SetExtraServicesApplied(extraServicesApplied);

            return Core.Utils.Optional.Optional<Order>.Of(order);
        }

        public async Task<Result<Order>> Save(Order order)
        {
            var mongoOrder = new MongoOrder
            {
                Id = order.GetId(),
                ContractClient = order.GetContractId(),
                DriverAssigned = order.GetDriverAssigned(),
                IncidentAddress = new MongoCoordinates
                {
                    Latitude = order.GetIncidentAddressLatitude(),
                    Longitude = order.GetIncidentAddressLongitude()
                },
                DestinationAddress = new MongoCoordinates
                {
                    Latitude = order.GetDestinationAddressLatitude(),
                    Longitude = order.GetDestinationAddressLongitude()
                },
                IncidentDate = order.GetIncidentDate(),
                ExtraServicesApplied = new List<MongoExtraServicesApplied>(),
                TotalCost = order.GetTotalCost(),
                Status = order.GetOrderStatus(),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var bsonDocument = new BsonDocument
            {
                {"_id", mongoOrder.Id},
                {"contractClient", mongoOrder.ContractClient},
                {"driverAssigned", mongoOrder.DriverAssigned},
                {"incidentAddress", new BsonDocument
                    {
                        {"latitude", mongoOrder.IncidentAddress.Latitude},
                        {"longitude", mongoOrder.IncidentAddress.Longitude}
                    }
                },
                {"destinationAddress", new BsonDocument
                    {
                        {"latitude", mongoOrder.DestinationAddress.Latitude},
                        {"longitude", mongoOrder.DestinationAddress.Longitude}
                    }
                },
                {"incidentDate", mongoOrder.IncidentDate},
                {"extraServicesApplied", new BsonArray()},
                {"totalCost", mongoOrder.TotalCost},
                {"status", mongoOrder.Status},
                {"createdDate", mongoOrder.CreatedDate},
                {"updatedDate", mongoOrder.UpdatedDate}
            };

            await _orderCollection.InsertOneAsync(bsonDocument);

            var savedOrder = Order.CreateOrder(
                new OrderId(mongoOrder.Id),
                new ContractId(mongoOrder.ContractClient),
                new DriverId(mongoOrder.DriverAssigned),
                new Coordinates(mongoOrder.IncidentAddress.Latitude, mongoOrder.IncidentAddress.Longitude),
                new Coordinates(mongoOrder.DestinationAddress.Latitude, mongoOrder.DestinationAddress.Longitude),
                new List<ExtraCost>()
            );

            return Result<Order>.Success(savedOrder);
        }

        public async Task<Result<Order>> UpdateDriverAssigned(Order order)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", order.GetId());
            var update = Builders<BsonDocument>.Update
                .Set("driverAssigned", order.GetDriverAssigned());

            var updateResult = await _orderCollection.UpdateOneAsync(filter, update);

            if (updateResult.ModifiedCount == 0)
            {
                return Result<Order>.Failure(new Exception("Failed to update order"));
            }

            return Result<Order>.Success(order);
        }

        public async Task<Result<Order>> UpdateOrderStatus(Order order)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", order.GetId());
            var update = Builders<BsonDocument>.Update
                .Set("status", order.GetOrderStatus());

            var updateResult = await _orderCollection.UpdateOneAsync(filter, update);

            if (updateResult.ModifiedCount == 0)
            {
                return Result<Order>.Failure(new Exception("Failed to update order"));
            }

            return Result<Order>.Success(order);
        }


        public async Task<Result<Order>> UpdateExtraCosts(OrderId orderId, List<ExtraCost> extraCosts)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", orderId.GetValue());
            var update = Builders<BsonDocument>.Update
                .Set("extraServicesApplied", new BsonArray(extraCosts.Select(c => new BsonDocument
                {
                    { "id", c.GetId() },
                    { "name", c.GetName() },
                    { "price", c.GetPrice() }
                })));

            var updateResult = await _orderCollection.UpdateOneAsync(filter, update);

            if (updateResult.ModifiedCount == 0)
            {
                return Result<Order>.Failure(new Exception("Failed to update list of extra services applied."));
            }

            var updatedOrderOptional = await GetById(orderId.GetValue());
            if (!updatedOrderOptional.HasValue)
            {
                return Result<Order>.Failure(new Exception("Order not found after update"));
            }

            var updatedOrder = updatedOrderOptional.Unwrap();
            return Result<Order>.Success(updatedOrder);
        }
    }
}

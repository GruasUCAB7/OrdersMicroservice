﻿using MassTransit;
using OrdersMS.Core.Application.GoogleApiService;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Commands.CreateOrder.Types;
using OrdersMS.src.Orders.Application.Events;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Application.Commands.CreateOrder
{
    public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IContractRepository contractRepository,
    IdGenerator<string> idGenerator,
    IGoogleApiService googleApiService,
    IPublishEndpoint publishEndpoint
) : IService<CreateOrderCommand, CreateOrderResponse>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IContractRepository _contractRepository = contractRepository;
        private readonly IdGenerator<string> _idGenerator = idGenerator;
        private readonly IGoogleApiService _googleApiService = googleApiService;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task<Result<CreateOrderResponse>> Execute(CreateOrderCommand data)
        {
            var isContractExist = await _contractRepository.GetById(data.ContractId);
            if (!isContractExist.HasValue)
            {
                throw new ContractNotFoundException();
            }
            var isContractIsActive = await _contractRepository.IsActiveContract(data.ContractId);
            if (isContractIsActive == false)
            {
                throw new ContractNotAvailableException();
            }

            var incidentCoordinatesResult = await _googleApiService.GetCoordinatesFromAddress(data.IncidentAddress);
            if (incidentCoordinatesResult == null)
            {
                return Result<CreateOrderResponse>.Failure(new CoordinatesNotFoundException("Incident coordinates not found."));
            }

            var destinationCoordinatesResult = await _googleApiService.GetCoordinatesFromAddress(data.DestinationAddress);
            if (destinationCoordinatesResult == null)
            {
                return Result<CreateOrderResponse>.Failure(new CoordinatesNotFoundException("Destination coordinates not found."));
            }

            var id = _idGenerator.Generate();
            var order = Order.CreateOrder(
                new OrderId(id),
                new ContractId(data.ContractId),
                new UserId(data.OperatorId),
                new DriverId("Por asignar"),
                new Coordinates(incidentCoordinatesResult.Unwrap().Latitude, incidentCoordinatesResult.Unwrap().Longitude),
                new Coordinates(destinationCoordinatesResult.Unwrap().Latitude, destinationCoordinatesResult.Unwrap().Longitude),
                new IncidentType(data.IncidentType),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(0),
                new OrderStatus("Por Asignar")
            );

            await _publishEndpoint.Publish(new OrderCreatedEvent(Guid.Parse(id)));

            await _orderRepository.Save(order);

            return Result<CreateOrderResponse>.Success(new CreateOrderResponse(id));
        }
    }
}

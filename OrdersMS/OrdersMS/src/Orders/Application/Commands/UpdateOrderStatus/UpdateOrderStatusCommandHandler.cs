using MassTransit;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatus.Types;
using OrdersMS.src.Orders.Application.Events;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain.ValueObjects;


namespace OrdersMS.src.Orders.Application.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IPublishEndpoint publishEndpoint) : IService<(string orderId, UpdateOrderStatusCommand data), GetOrderResponse>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task<Result<GetOrderResponse>> Execute((string orderId, UpdateOrderStatusCommand data) request)
        {
            var orderOptional = await _orderRepository.GetById(request.orderId);
            if (!orderOptional.HasValue)
            {
                return Result<GetOrderResponse>.Failure(new OrderNotFoundException());
            }

            var order = orderOptional.Unwrap();

            if (request.data.OrderAcceptedDriverResponse != null) 
            {
                if (request.data.OrderAcceptedDriverResponse == true)
                {
                    order.SetStatus(new OrderStatus("Aceptado"));
                    await _publishEndpoint.Publish(new DriverAcceptedOrderEvent(Guid.Parse(order.GetId())));
                }

                if (request.data.OrderAcceptedDriverResponse == false)
                {
                    order.SetStatus(new OrderStatus("Por Asignar"));
                    order.SetDriverAssigned(new DriverId("null"));
                    await _publishEndpoint.Publish(new DriverRefusedOrderEvent(Guid.Parse(order.GetId())));
                }
            }

            if (request.data.OrderInProcessDriverResponse != null)
            {
                if (request.data.OrderInProcessDriverResponse == true)
                {
                    order.SetStatus(new OrderStatus("En Proceso"));
                    await _publishEndpoint.Publish(new OrderInProcessEvent(Guid.Parse(order.GetId())));
                }
            }

            if (request.data.OrderCanceledDriverResponse != null)
            {
                if (request.data.OrderCanceledDriverResponse == true)
                {
                    order.SetStatus(new OrderStatus("Cancelada"));
                    await _publishEndpoint.Publish(new OrderCanceledEvent(Guid.Parse(order.GetId())));
                }
            }

            var updateResult = await _orderRepository.Update(order);
            if (updateResult.IsFailure)
            {
                return Result<GetOrderResponse>.Failure(new OrderUpdateFailedException("Order status could not be updated correctly"));
            }

            var extraServices = order.GetExtrasServicesApplied().Select(extraCost => new ExtraServiceDto(
                extraCost.GetId(),
                extraCost.GetName(),
                extraCost.GetPrice()
            )).ToList();

            var response = new GetOrderResponse(
                order.GetId(),
                order.GetContractId(),
                order.GetOperatorAssigned(),
                order.GetDriverAssigned(),
                new CoordinatesDto(order.GetIncidentAddressLatitude(), order.GetIncidentAddressLongitude()),
                new CoordinatesDto(order.GetDestinationAddressLatitude(), order.GetDestinationAddressLongitude()),
                order.GetIncidentType(),
                order.GetIncidentDate(),
                extraServices,
                order.GetTotalCost(),
                order.GetOrderStatus()
            );

            return Result<GetOrderResponse>.Success(response);
        }
    }
}

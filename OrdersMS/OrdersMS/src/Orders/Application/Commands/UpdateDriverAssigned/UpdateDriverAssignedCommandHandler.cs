using MassTransit;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Commands.UpdateDriverAssigned.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Events;

namespace OrdersMS.src.Orders.Application.Commands.UpdateDriverAssigned
{
    public class UpdateDriverAssignedCommandHandler(
        IOrderRepository orderRepository, 
        IPublishEndpoint publishEndpoint
    ) : IService<(string orderId, UpdateDriverAssignedCommand data), GetOrderResponse>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task<Result<GetOrderResponse>> Execute((string orderId, UpdateDriverAssignedCommand data) request)
        {
            var orderOptional = await _orderRepository.GetById(request.orderId);
            if (!orderOptional.HasValue)
            {
                return Result<GetOrderResponse>.Failure(new OrderNotFoundException());
            }

            var order = orderOptional.Unwrap();

            if (order.GetOrderStatus() != "Por Asignar")
            {
                return Result<GetOrderResponse>.Failure(new OrderUpdateFailedException("The order is not in the Por Asignar status"));
            }

            if (!string.IsNullOrEmpty(request.data.DriverAssigned))
            {
                order.SetDriverAssigned(new DriverId(request.data.DriverAssigned));
            }
            order.SetStatus(new OrderStatus("Por Aceptar"));


            var updateResult = await _orderRepository.Update(order);
            if (updateResult.IsFailure)
            {
                return Result<GetOrderResponse>.Failure(new OrderUpdateFailedException("The driver assigned of the order could not be updated correctly"));
            }

            await _publishEndpoint.Publish(new DriverAssignedToOrderEvent(Guid.Parse(order.GetId())));

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

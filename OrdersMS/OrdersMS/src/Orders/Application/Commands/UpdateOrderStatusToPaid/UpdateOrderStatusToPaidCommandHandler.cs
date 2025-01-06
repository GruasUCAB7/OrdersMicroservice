using MassTransit;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToPaid.Types;
using OrdersMS.src.Orders.Application.Events;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToPaid
{
    public class UpdateOrderStatusToPaidCommandHandler(IOrderRepository orderRepository, IPublishEndpoint publishEndpoint) : IService<(string orderId, UpdateOrderStatusToPaidCommand data), GetOrderResponse>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task<Result<GetOrderResponse>> Execute((string orderId, UpdateOrderStatusToPaidCommand data) request)
        {
            var orderOptional = await _orderRepository.GetById(request.orderId);
            if (!orderOptional.HasValue)
            {
                return Result<GetOrderResponse>.Failure(new OrderNotFoundException());
            }

            var order = orderOptional.Unwrap();


            if (request.data.OrderPaidResponse == true)
            {
                if (order.GetOrderStatus() != "Finalizado")
                {
                    return Result<GetOrderResponse>.Failure(new OrderUpdateFailedException("The order is not in the Finalizado status"));
                }

                order.SetStatus(new OrderStatus("Pagado"));
                await _publishEndpoint.Publish(new OrderPaidEvent(Guid.Parse(order.GetId())));
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

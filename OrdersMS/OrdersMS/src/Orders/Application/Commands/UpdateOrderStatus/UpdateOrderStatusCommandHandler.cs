using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Commands.UpdateDriverAssigned.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatus.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain.ValueObjects;
using RestSharp;
using System.Text.Json;


namespace OrdersMS.src.Orders.Application.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandHandler(IOrderRepository orderRepository) : IService<(string orderId, UpdateOrderStatusCommand data), GetOrderResponse>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;

        public async Task<Result<GetOrderResponse>> Execute((string orderId, UpdateOrderStatusCommand data) request)
        {
            var orderOptional = await _orderRepository.GetById(request.orderId);
            if (!orderOptional.HasValue)
            {
                return Result<GetOrderResponse>.Failure(new OrderNotFoundException());
            }

            var order = orderOptional.Unwrap();

            if (!string.IsNullOrEmpty(request.data.Status))
            {
                var status = new OrderStatus(request.data.Status);
                order.SetStatus(status);
            }

            var updateResult = await _orderRepository.UpdateOrderStatus(order);
            if (updateResult.IsFailure)
            {
                return Result<GetOrderResponse>.Failure(new OrderUpdateFailedException("Order could not be updated correctly"));
            }

            var extraServices = order.GetExtrasServicesApplied().Select(extraCost => new ExtraServiceDto(
                extraCost.GetId(),
                extraCost.GetName(),
                extraCost.GetPrice()
            )).ToList();

            var response = new GetOrderResponse(
                order.GetId(),
                order.GetContractId(),
                order.GetDriverAssigned(),
                new CoordinatesDto(order.GetIncidentAddressLatitude(), order.GetIncidentAddressLongitude()),
                new CoordinatesDto(order.GetDestinationAddressLatitude(), order.GetDestinationAddressLongitude()),
                order.GetIncidentDate(),
                extraServices,
                order.GetTotalCost(),
                order.GetOrderStatus()
            );

            return Result<GetOrderResponse>.Success(response);
        }
    }
}

using MassTransit;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToCompleted.Types;
using OrdersMS.src.Orders.Application.Events;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain.ValueObjects;
using RestSharp;

namespace OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToCompleted
{
    public class UpdateOrderStatusToCompletedCommandHandler(IOrderRepository orderRepository, IPublishEndpoint publishEndpoint) : IService<(string orderId, UpdateOrderStatusToCompletedCommand data), GetOrderResponse>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task<Result<GetOrderResponse>> Execute((string orderId, UpdateOrderStatusToCompletedCommand data) request)
        {
            var orderOptional = await _orderRepository.GetById(request.orderId);
            if (!orderOptional.HasValue)
            {
                return Result<GetOrderResponse>.Failure(new OrderNotFoundException());
            }

            var order = orderOptional.Unwrap();


            if (request.data.OrderCompletedDriverResponse == true)
            {
                order.SetStatus(new OrderStatus("Finalizado"));
                await _publishEndpoint.Publish(new OrderCompletedEvent(Guid.Parse(order.GetId())));

                var client = new RestClient("https://localhost:4052");
                var changeIsAvailableToTrueConductor = new RestRequest($"/provider/driver/{request.data.DriverAssigned}", Method.Patch);
                changeIsAvailableToTrueConductor.AddJsonBody(new { isAvailable = true });

                var responseDriver = await client.ExecuteAsync(changeIsAvailableToTrueConductor);
                if (!responseDriver.IsSuccessful)
                {
                    return Result<GetOrderResponse>.Failure(new Exception("Failed to update driver availability"));
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

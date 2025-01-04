using MassTransit;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Commands.ValidateLocationDriverToIncidecident.Types;
using OrdersMS.src.Orders.Application.Events;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Application.Commands.ValidateLocationDriverToIncidecident
{
    public class ValidateLocationDriverToIncidecidentCommandHandler(
        IOrderRepository orderRepository, 
        IPublishEndpoint publishEndpoint
    ): IService<(string orderId, ValidateLocationCommand data), GetOrderResponse>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task<Result<GetOrderResponse>> Execute((string orderId, ValidateLocationCommand data) request)
        {
            var orderOptional = await _orderRepository.GetById(request.orderId);
            if (!orderOptional.HasValue)
            {
                return Result<GetOrderResponse>.Failure(new OrderNotFoundException());
            }
            var order = orderOptional.Unwrap();

            if (request.data.DriverLocationResponse == true)
            {
                order.SetStatus(new OrderStatus("Localizado"));
                await _publishEndpoint.Publish(new DriverIsAtTheIncidentEvent(Guid.Parse(order.GetId())));
            }

            var updateResult = await _orderRepository.Update(order);
            if (updateResult.IsFailure)
            {
                return Result<GetOrderResponse>.Failure(new OrderUpdateFailedException("The driver assigned of the order could not be updated correctly"));
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

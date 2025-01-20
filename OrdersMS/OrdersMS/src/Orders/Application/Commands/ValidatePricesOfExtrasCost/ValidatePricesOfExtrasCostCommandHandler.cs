using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost.Types;
using OrdersMS.src.Orders.Application.Commands.ValidatePricesOfExtrasCost.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Application.Commands.ValidatePricesOfExtrasCost
{
    public class ValidatePricesOfExtrasCostCommandHandler(
        IOrderRepository orderRepository,
        AddExtraCostCommandHandler addExtraCostService
    ) : IService<(string orderId, ValidatePricesOfExtrasCostCommand data), GetOrderResponse>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly AddExtraCostCommandHandler _addExtraCostService = addExtraCostService;

        public async Task<Result<GetOrderResponse>> Execute((string orderId, ValidatePricesOfExtrasCostCommand data) request)
        {
            var orderOptional = await _orderRepository.GetById(request.orderId);
            if (!orderOptional.HasValue)
            {
                return Result<GetOrderResponse>.Failure(new OrderNotFoundException());
            }
            var order = orderOptional.Unwrap();

            if (request.data.OperatorRespose == true)
            {
                if (request.data.ExtrasCostApplied != null)
                {
                    var addExtraCostCommand = new AddExtraCostCommand(request.data.ExtrasCostApplied);

                    var addExtraCostResult = await _addExtraCostService.Execute((request.orderId, addExtraCostCommand));
                    if (addExtraCostResult.IsFailure)
                    {
                        return Result<GetOrderResponse>.Failure(new FailedToAddingExtraCostExtraCost());
                    }
                    var extraCosts = request.data.ExtrasCostApplied.Select(dto => new ExtraCost(
                        new ExtraCostId(dto.Id),
                        new OrderId(request.orderId),
                        new ExtraCostName(dto.Name),
                        new ExtraCostPrice(dto.Price)
                    )).ToList();
                    order.SetExtraServicesApplied(extraCosts);
                }
            }

            var updateResult = await _orderRepository.Update(order);
            if (updateResult.IsFailure)
            {
                return Result<GetOrderResponse>.Failure(new OrderUpdateFailedException("The order could not be updated correctly"));
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

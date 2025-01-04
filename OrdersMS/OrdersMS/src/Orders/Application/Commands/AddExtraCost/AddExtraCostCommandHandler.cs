using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Application.Validators;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.Exceptions;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Application.Commands.AddExtraCost
{
    public class AddExtraCostCommandHandler(
        IOrderRepository orderRepository,
        IdGenerator<string> idGenerator
    ) : IService<(string orderId, AddExtraCostCommand data), GetOrderResponse>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IdGenerator<string> _idGenerator = idGenerator;

        public async Task<Result<GetOrderResponse>> Execute((string orderId, AddExtraCostCommand data) request)
        {
            var orderExist = await _orderRepository.GetById(request.orderId);
            if (!orderExist.HasValue)
            {
                return Result<GetOrderResponse>.Failure(new OrderNotFoundException());
            }
            var order = orderExist.Unwrap();
            var extraCosts = new List<ExtraCost>();

            foreach (var extraCost in request.data.ExtraCosts)
            {
                if (!ExtraCostValidator.ValidateExtraCostName(extraCost.Name))
                {
                    return Result<GetOrderResponse>.Failure(new InvalidExtraCostNameException(extraCost.Name));
                }
                var id = _idGenerator.Generate();
                var extraCostAdded = order.AddExtraCost(new ExtraCostId(id), new ExtraCostName(extraCost.Name), new ExtraCostPrice(extraCost.Price));
                extraCosts.Add(extraCostAdded);
            }

            var updateResult = await _orderRepository.UpdateExtraCosts(new OrderId(request.orderId), extraCosts);
            if (!updateResult.IsSuccessful)
            {
                return Result<GetOrderResponse>.Failure(new ExtraServicesAppliedUpdateFailedException());
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

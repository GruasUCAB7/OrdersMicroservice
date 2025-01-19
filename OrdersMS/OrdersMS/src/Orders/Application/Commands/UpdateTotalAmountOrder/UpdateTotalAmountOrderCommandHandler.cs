using MassTransit;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Orders.Application.Commands.UpdateTotalAmountOrder.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.Services;
using OrdersMS.src.Orders.Domain.Services.Types;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Application.Commands.UpdateTotalAmountOrder
{
    public class UpdateTotalAmountOrderCommandHandler(
        IOrderRepository orderRepository, 
        IContractRepository contractRepository,
        IPolicyRepository policyRepository,
        CalculateOrderTotalAmount calculateOrderTotalAmount) : IService<(string orderId, UpdateTotalAmountOrderCommand data), GetOrderResponse>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IContractRepository _contractRepository = contractRepository;
        private readonly IPolicyRepository _policyRepository = policyRepository;
        private readonly CalculateOrderTotalAmount _calculateOrderTotalAmount = calculateOrderTotalAmount;

        public async Task<Result<GetOrderResponse>> Execute((string orderId, UpdateTotalAmountOrderCommand data) request)
        {
            var orderOptional = await _orderRepository.GetById(request.orderId);
            if (!orderOptional.HasValue)
            {
                return Result<GetOrderResponse>.Failure(new OrderNotFoundException());
            }
            var order = orderOptional.Unwrap();

            var listExtraCostApplied = order.GetExtrasServicesApplied()
                .Select(extraService => new ExtraCost(
                    extraService.Id,
                    new OrderId(order.GetId()),
                    new ExtraCostName(extraService.GetName()),
                    new ExtraCostPrice(extraService.GetPrice())
                ))
                .ToList();

            var priceListExtraCostApplied = listExtraCostApplied.Select(priceExtraCost => priceExtraCost.GetPrice()).ToList();

            var contractId = order.GetContractId();
            var contractOptional = await _contractRepository.GetById(contractId);
            if (!contractOptional.HasValue)
            {
                return Result<GetOrderResponse>.Failure(new ContractNotFoundException());
            }

            var contract = contractOptional.Unwrap();

            var policyId = contract.GetPolicyId();
            var policyOptional = await _policyRepository.GetById(policyId);
            if (!policyOptional.HasValue)
            {
                return Result<GetOrderResponse>.Failure(new PolicyNotFoundException());
            }

            var policy = policyOptional.Unwrap();

            var policyKmCoverage = policy.GetPolicyCoverageKm();
            var policyAmountCoverage = policy.GetPolicyIncidentCoverageAmount();
            var extraKmPrice = policy.GetPriceExtraKm();

            var input = new CalculateOrderTotalAmountInput(
                request.data.TotalKmTraveled,
                policyKmCoverage,
                policyAmountCoverage,
                extraKmPrice,
                priceListExtraCostApplied
            );

            var totalCost = _calculateOrderTotalAmount.Execute(input);

            order.SetTotalCost(totalCost);

            var updateResult = await _orderRepository.Update(order);
            if (updateResult.IsFailure)
            {
                return Result<GetOrderResponse>.Failure(new OrderUpdateFailedException("Order total amount could not be updated correctly"));
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

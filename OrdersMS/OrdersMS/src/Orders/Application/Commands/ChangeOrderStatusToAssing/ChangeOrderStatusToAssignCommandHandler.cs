using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Application.Commands.ChangeOrderStatusToAssing
{
    public class ChangeOrderStatusToAssignCommandHandler(IOrderRepository orderRepository)
    {
        private readonly IOrderRepository _orderRepository = orderRepository;

        public async Task<Result<List<Order>>> Execute()
        {
            var modifiedOrders = await _orderRepository.ValidateUpdateTimeForStatusPorAceptar();
            var originalOrders = new List<Order>();

            foreach (var order in modifiedOrders)
            {
                var driverId = order.GetDriverAssigned();
                if (driverId != null)
                {
                    var extraServices = order.GetExtrasServicesApplied().Select(extraCost => new ExtraCost(
                        new ExtraCostId(extraCost.GetId()),
                        new OrderId(order.GetId()),
                        new ExtraCostName(extraCost.GetName()),
                        new ExtraCostPrice(extraCost.GetPrice())
                    )).ToList();

                    var originalOrder = Order.CreateOrder(
                        new OrderId(order.GetId()),
                        new ContractId(order.GetContractId()),
                        new UserId(order.GetOperatorAssigned()),
                        new DriverId(order.GetDriverAssigned()),
                        new Coordinates(order.GetIncidentAddressLatitude(), order.GetIncidentAddressLongitude()),
                        new Coordinates(order.GetDestinationAddressLatitude(), order.GetDestinationAddressLongitude()),
                        new IncidentType(order.GetIncidentType()),
                        order.GetIncidentDate(),
                        extraServices,
                        new TotalCost(order.GetTotalCost()),
                        new OrderStatus(order.GetOrderStatus())
                    );
                    originalOrders.Add(originalOrder);

                    order.SetDriverAssigned(new DriverId("Por asignar"));

                    var updateResult = await _orderRepository.Update(order);
                    if (updateResult.IsFailure)
                    {
                        return Result<List<Order>>.Failure(new OrderUpdateFailedException("Order could not be updated correctly"));
                    }
                }
            }
            return Result<List<Order>>.Success(originalOrders);
        }
    }
}

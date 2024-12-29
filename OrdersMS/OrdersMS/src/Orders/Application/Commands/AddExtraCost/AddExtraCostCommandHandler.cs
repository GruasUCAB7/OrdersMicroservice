using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Application.Validators;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.Exceptions;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Application.Commands.AddExtraCost
{
    public class AddExtraCostCommandHandler(
        IOrderRepository orderRepository,
        IdGenerator<string> idGenerator
    ) : IService<AddExtraCostCommand, AddExtraCostResponse>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IdGenerator<string> _idGenerator = idGenerator;

        public async Task<Result<AddExtraCostResponse>> Execute(AddExtraCostCommand data)
        {
            var orderExist = await _orderRepository.GetById(data.OrderId);
            if (!orderExist.HasValue)
            {
                return Result<AddExtraCostResponse>.Failure(new OrderNotFoundException());
            }
            var order = orderExist.Unwrap();
            var extraCosts = new List<ExtraCost>();

            foreach (var extraCost in data.ExtraCosts)
            {
                if (!ExtraCostValidator.ValidateExtraCostName(extraCost.Name))
                {
                    return Result<AddExtraCostResponse>.Failure(new InvalidExtraCostNameException(extraCost.Name));
                }
                var id = _idGenerator.Generate();
                var extraCostAdded = order.AddExtraCost(new ExtraCostId(id), new ExtraCostName(extraCost.Name), new ExtraCostPrice(extraCost.Price));
                extraCosts.Add(extraCostAdded);
            }

            var updateResult = await _orderRepository.UpdateExtraCosts(new OrderId(data.OrderId), extraCosts);
            if (!updateResult.IsSuccessful)
            {
                return Result<AddExtraCostResponse>.Failure(new ExtraServicesAppliedUpdateFailedException());
            }

            return Result<AddExtraCostResponse>.Success(new AddExtraCostResponse(order.GetId()));
        }
    }
}

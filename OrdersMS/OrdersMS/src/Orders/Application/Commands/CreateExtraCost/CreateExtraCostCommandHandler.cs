using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Commands.CreateExtraCost.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Application.Commands.CreateExtraCost
{
    public class CreateExtraCostCommandHandler(IExtraCostRepository extraCostRepository, IOrderRepository orderRepository, IdGenerator<string> idGenerator)
    {
        private readonly IExtraCostRepository _extraCostRepository = extraCostRepository;
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IdGenerator<string> _idGenerator = idGenerator;

        public async Task<Result<List<ExtraCost>>> Execute(CreateExtraCostCommand data)
        {
            var isOrderExist = await _orderRepository.GetById(data.OrderId);
            if (!isOrderExist.HasValue)
            {
                throw new OrderNotFoundException();
            }

            
            var extraCosts = new List<ExtraCost>();

            foreach (var extra in data.ExtraCosts)
            {
                var id = _idGenerator.Generate();
                var extraCost = new ExtraCost(new ExtraCostId(id), new OrderId(data.OrderId), new ExtraCostName(extra.Name), new ExtraCostPrice(extra.Price));
                var result = await _extraCostRepository.Save(extraCost);
            }

            return Result<List<ExtraCost>>.Success(extraCosts);
        }
    }
}

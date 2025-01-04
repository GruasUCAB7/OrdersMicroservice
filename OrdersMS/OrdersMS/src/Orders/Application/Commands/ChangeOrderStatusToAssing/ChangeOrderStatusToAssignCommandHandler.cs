using OrdersMS.src.Orders.Application.Repositories;

namespace OrdersMS.src.Orders.Application.Commands.ChangeOrderStatusToAssing
{
    public class ChangeOrderStatusToAssignCommandHandler(IOrderRepository orderRepository)
    {
        private readonly IOrderRepository _orderRepository = orderRepository;

        public async Task<bool> Execute()
        {
            await _orderRepository.ValidateUpdateTimeForStatusPorAceptar();
            return true;
        }
    }
}

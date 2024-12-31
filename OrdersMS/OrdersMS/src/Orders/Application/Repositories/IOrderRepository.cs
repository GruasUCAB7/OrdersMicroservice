using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders.Types;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Application.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAll(GetAllOrdersQuery data);
        Task<Optional<Order>> GetById(string id);
        Task<Result<Order>> Save(Order order);
        Task<Result<Order>> UpdateDriverAssigned(Order order);
        Task<Result<Order>> UpdateExtraCosts(OrderId orderId, List<ExtraCost> extraCosts);
        Task<Result<Order>> UpdateOrderStatus(Order order);
    }
}

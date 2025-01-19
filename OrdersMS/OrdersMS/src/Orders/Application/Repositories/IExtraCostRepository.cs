using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Domain.Entities;

namespace OrdersMS.src.Orders.Application.Repositories
{
    public interface IExtraCostRepository
    {
        Task<List<ExtraCost>> GetExtraCostByOrderId(string orderId);
        Task<Result<ExtraCost>> Save(ExtraCost extraCost);
    }
}

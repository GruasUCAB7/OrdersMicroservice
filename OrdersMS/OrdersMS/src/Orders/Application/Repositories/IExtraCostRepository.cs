using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Queries.GetAllExtraCosts.Types;
using OrdersMS.src.Orders.Domain.Entities;

namespace OrdersMS.src.Orders.Application.Repositories
{
    public interface IExtraCostRepository
    {
        Task<bool> ExistByName(string name);
        Task<List<ExtraCost>> GetAll(GetAllExtraCostsQuery data);
        Task<Optional<ExtraCost>> GetById(string id);
        Task<Result<ExtraCost>> Save(ExtraCost extraCost);
        Task<Result<ExtraCost>> Update(ExtraCost extraCost);
        Task<bool> IsActiveExtraCost(string id);
    }
}

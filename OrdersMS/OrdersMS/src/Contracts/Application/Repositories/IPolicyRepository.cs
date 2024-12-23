using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Queries.GetAllPolicies.Types;
using OrdersMS.src.Contracts.Domain.Entities;

namespace OrdersMS.src.Contracts.Application.Repositories
{
    public interface IPolicyRepository
    {
        Task<bool> ExistByType(string type);
        Task<List<InsurancePolicy>> GetAll(GetAllPoliciesQuery data);
        Task<Optional<InsurancePolicy>> GetById(string id);
        Task<Result<InsurancePolicy>> Save(InsurancePolicy policy);
        Task<Result<InsurancePolicy>> Update(InsurancePolicy policy);
    }
}

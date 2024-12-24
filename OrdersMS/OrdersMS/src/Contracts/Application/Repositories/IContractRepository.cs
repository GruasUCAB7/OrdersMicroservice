using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Queries.GetAllContracts.Types;
using OrdersMS.src.Contracts.Domain;

namespace OrdersMS.src.Contracts.Application.Repositories
{
    public interface IContractRepository
    {
        Task<List<Contract>> GetAll(GetAllContractsQuery data);
        Task<Optional<Contract>> GetById(string id);
        Task<Result<Contract>> Save(Contract contract);
        Task<Result<Contract>> Update(Contract contract);
        Task<bool> ContractExists(string policyId, string vehicleId);
    }
}

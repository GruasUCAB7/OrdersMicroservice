using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Queries.GetAllContracts.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Application.Types;

namespace OrdersMS.src.Contracts.Application.Queries.GetAllContracts
{
    public class GetAllContractsQueryHandler(IContractRepository contractRepository) : IService<GetAllContractsQuery, GetContractResponse[]>
    {
        private readonly IContractRepository _contractRepository = contractRepository;
        public async Task<Result<GetContractResponse[]>> Execute(GetAllContractsQuery data)
        {
            var contract = await _contractRepository.GetAll(data);
            if (contract == null)
            {
                return Result<GetContractResponse[]>.Failure(new ContractNotFoundException());
            }

            var response = contract.Select(contract => new GetContractResponse(
                contract.GetId(),
                contract.GetPolicyId(),
                contract.GetVehicleId(),
                contract.GetStartDate(),
                contract.GetExpirationDate(),
                contract.GetStatus()
                )
            ).ToArray();

            return Result<GetContractResponse[]>.Success(response);
        }
    }
}

using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Queries.GetContractById.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Application.Types;

namespace OrdersMS.src.Contracts.Application.Queries.GetContractById
{
    public class GetContractByIdQueryHandler(IContractRepository contractRepository) : IService<GetContractByIdQuery, GetContractResponse>
    {
        private readonly IContractRepository _contractRepository = contractRepository;
        public async Task<Result<GetContractResponse>> Execute(GetContractByIdQuery data)
        {
            var vehicleOptional = await _contractRepository.GetById(data.Id);
            if (!vehicleOptional.HasValue)
            {
                return Result<GetContractResponse>.Failure(new ContractNotFoundException());
            }

            var contract = vehicleOptional.Unwrap();
            var response = new GetContractResponse(
                contract.GetId(),
                contract.GetContractNumber(),
                contract.GetPolicyId(),
                contract.GetVehicleId(),
                contract.GetStartDate(),
                contract.GetExpirationDate(),
                contract.GetStatus()
            );

            return Result<GetContractResponse>.Success(response);
        }
    }
}

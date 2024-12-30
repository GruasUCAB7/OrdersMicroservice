using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Queries.GetContractId.Types;
using OrdersMS.src.Contracts.Application.Repositories;

namespace OrdersMS.src.Contracts.Application.Queries.GetContractId
{
    public class GetContractIdQueryHandler(IContractRepository contractRepository) : IService<GetContractIdQuery, GetContractIdResponse>
    {
        private readonly IContractRepository _contractRepository = contractRepository;
        public async Task<Result<GetContractIdResponse>> Execute(GetContractIdQuery data)
        {
            var contractIdOptional = await _contractRepository.GetContractIdByContractNumber(data.ContractNumber);
            if (string.IsNullOrEmpty(contractIdOptional))
            {
                return Result<GetContractIdResponse>.Failure(new ContractNotFoundException());
            }

            var response = new GetContractIdResponse(contractIdOptional);

            return Result<GetContractIdResponse>.Success(response);
        }
    }
}

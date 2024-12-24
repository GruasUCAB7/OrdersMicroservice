using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Commands.CreateExtraCost.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Application.Commands.CreateExtraCost
{

    public class CreateExtraCostCommandHandler(
        IExtraCostRepository extraCostRepository,
        IdGenerator<string> idGenerator
    ) : IService<CreateExtraCostCommand, CreateExtraCostResponse>
    {
        private readonly IExtraCostRepository _extraCostRepository = extraCostRepository;
        private readonly IdGenerator<string> _idGenerator = idGenerator;

        public async Task<Result<CreateExtraCostResponse>> Execute(CreateExtraCostCommand data)
        {
            var isExtraCostExist = await _extraCostRepository.ExistByName(data.Name);
            if (isExtraCostExist)
            {
                return Result<CreateExtraCostResponse>.Failure(new ExtraCostAlreadyExistException(data.Name));
            }

            var id = _idGenerator.Generate();
            var extraCost = new ExtraCost(
                new ExtraCostId(id),
                new ExtraCostName(data.Name)
            );
            await _extraCostRepository.Save(extraCost);

            return Result<CreateExtraCostResponse>.Success(new CreateExtraCostResponse(id));
        }
    }
}

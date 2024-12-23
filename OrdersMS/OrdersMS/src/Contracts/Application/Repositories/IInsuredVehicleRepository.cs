using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Queries.GetAllVehicles.Types;
using OrdersMS.src.Contracts.Domain.Entities;

namespace OrdersMS.src.Contracts.Application.Repositories
{
    public interface IInsuredVehicleRepository
    {
        Task<bool> ExistByPlate(string plate);
        Task<List<InsuredVehicle>> GetAll(GetAllVehiclesQuery data);
        Task<Optional<InsuredVehicle>> GetById(string id);
        Task<Result<InsuredVehicle>> Save(InsuredVehicle vehicle);
        Task<Result<InsuredVehicle>> Update(InsuredVehicle vehicle);
    }
}

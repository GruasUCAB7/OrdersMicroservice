using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.InsuredVehicles.Application.Queries.GetAll.Types;
using OrdersMS.src.InsuredVehicles.Domain;

namespace OrdersMS.src.InsuredVehicles.Application.Repositories
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

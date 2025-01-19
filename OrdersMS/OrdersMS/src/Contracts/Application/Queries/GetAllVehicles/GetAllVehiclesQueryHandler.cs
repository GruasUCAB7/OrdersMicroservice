using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Queries.GetAllVehicles.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Application.Types;

namespace OrdersMS.src.Contracts.Application.Queries.GetAllVehicles
{
    public class GetAllVehiclesQueryHandler(IInsuredVehicleRepository vehicleRepository) : IService<GetAllVehiclesQuery, GetVehicleResponse[]>
    {
        private readonly IInsuredVehicleRepository _vehicleRepository = vehicleRepository;
        public async Task<Result<GetVehicleResponse[]>> Execute(GetAllVehiclesQuery data)
        {
            var vehicle = await _vehicleRepository.GetAll(data);
            if (vehicle == null)
            {
                return Result<GetVehicleResponse[]>.Failure(new VehicleNotFoundException());
            }

            var response = vehicle.Select(vehicle => new GetVehicleResponse(
                vehicle.GetId(),
                vehicle.GetBrand(),
                vehicle.GetModel(),
                vehicle.GetPlate(),
                vehicle.GetVehicleSize(),
                vehicle.GetYear(),
                vehicle.GetIsActive(),
                vehicle.GetClientDNI(),
                vehicle.GetClientName(),
                vehicle.GetClientEmail()
                )
            ).ToArray();

            return Result<GetVehicleResponse[]>.Success(response);
        }
    }
}

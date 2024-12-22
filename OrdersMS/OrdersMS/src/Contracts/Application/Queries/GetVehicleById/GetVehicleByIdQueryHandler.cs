using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Queries.GetVehicleById.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Application.Types;

namespace OrdersMS.src.Contracts.Application.Queries.GetVehicleById
{
    public class GetVehicleByIdQueryHandler(IInsuredVehicleRepository vehicleRepository) : IService<GetVehicleByIdQuery, GetVehicleResponse>
    {
        private readonly IInsuredVehicleRepository _vehicleRepository = vehicleRepository;
        public async Task<Result<GetVehicleResponse>> Execute(GetVehicleByIdQuery data)
        {
            var vehicleOptional = await _vehicleRepository.GetById(data.Id);
            if (!vehicleOptional.HasValue)
            {
                return Result<GetVehicleResponse>.Failure(new VehicleNotFoundException());
            }

            var vehicle = vehicleOptional.Unwrap();
            var response = new GetVehicleResponse(
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
            );

            return Result<GetVehicleResponse>.Success(response);
        }
    }
}

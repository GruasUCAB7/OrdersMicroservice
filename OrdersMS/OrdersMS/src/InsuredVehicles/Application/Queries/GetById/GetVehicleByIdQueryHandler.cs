using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.InsuredVehicles.Application.Exceptions;
using OrdersMS.src.InsuredVehicles.Application.Queries.GetById.Types;
using OrdersMS.src.InsuredVehicles.Application.Repositories;
using OrdersMS.src.InsuredVehicles.Application.Types;

namespace OrdersMS.src.InsuredVehicles.Application.Queries.GetById
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

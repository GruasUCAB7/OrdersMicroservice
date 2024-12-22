using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredVehicle.Types;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Repositories;

namespace OrdersMS.src.Contracts.Application.Commands.UpdateInsuredVehicle
{
    public class UpdateVehicleCommandHandler(IInsuredVehicleRepository vehicleRepository) : IService<(string id, UpdateVehicleCommand data), UpdateVehicleResponse>
    {
        private readonly IInsuredVehicleRepository _vehicleRepository = vehicleRepository;

        public async Task<Result<UpdateVehicleResponse>> Execute((string id, UpdateVehicleCommand data) request)
        {
            var vehicleOptional = await _vehicleRepository.GetById(request.id);
            if (!vehicleOptional.HasValue)
            {
                return Result<UpdateVehicleResponse>.Failure(new VehicleNotFoundException());
            }

            var vehicle = vehicleOptional.Unwrap();

            if (request.data.IsActive.HasValue)
            {
                vehicle.SetIsActive(request.data.IsActive.Value);
            }

            var updateResult = await _vehicleRepository.Update(vehicle);
            if (updateResult.IsFailure)
            {
                return Result<UpdateVehicleResponse>.Failure(new VehicleUpdateFailedException());
            }

            var response = new UpdateVehicleResponse(
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

            return Result<UpdateVehicleResponse>.Success(response);
        }
    }
}

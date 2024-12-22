using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.InsuredVehicles.Application.Commands.CreateInsuredVehicle.Types;
using OrdersMS.src.InsuredVehicles.Application.Exceptions;
using OrdersMS.src.InsuredVehicles.Application.Repositories;
using OrdersMS.src.InsuredVehicles.Domain;
using OrdersMS.src.InsuredVehicles.Domain.ValueObjects;

namespace OrdersMS.src.InsuredVehicles.Application.Commands.CreateInsuredVehicle
{
    public class CreateVehicleCommandHandler(
        IInsuredVehicleRepository vehicleRepository,
        IdGenerator<string> idGenerator
    ) : IService<CreateVehicleCommand, CreateVehicleResponse>
    {
        private readonly IInsuredVehicleRepository _vehicleRepository = vehicleRepository;
        private readonly IdGenerator<string> _idGenerator = idGenerator;

        public async Task<Result<CreateVehicleResponse>> Execute(CreateVehicleCommand data)
        {
            var isVehicleExist = await _vehicleRepository.ExistByPlate(data.Plate);
            if (isVehicleExist)
            {
                return Result<CreateVehicleResponse>.Failure(new VehicleAlreadyExistException(data.Plate));
            }

            var id = _idGenerator.Generate();
            var vehicleSize = (VehicleSize)Enum.Parse(typeof(VehicleSize), data.VehicleSize);
            var vehicle = InsuredVehicle.CreateVehicle(
                new VehicleId(id),
                new VehicleBrand(data.Brand),
                new VehicleModel(data.Model),
                new VehiclePlate(data.Plate),
                vehicleSize,
                new VehicleYear(data.Year),
                new ClientDNI(data.ClientDNI),
                new ClientName(data.ClientName),
                new ClientEmail(data.ClientEmail)
            );
            await _vehicleRepository.Save(vehicle);

            return Result<CreateVehicleResponse>.Success(new CreateVehicleResponse(id));
        }
    }
}

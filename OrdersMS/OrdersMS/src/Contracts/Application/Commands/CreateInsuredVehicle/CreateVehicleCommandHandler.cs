using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.CreateInsuredVehicle.Types;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;

namespace OrdersMS.src.Contracts.Application.Commands.CreateInsuredVehicle
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
            var vehicle = new InsuredVehicle(
                new VehicleId(id),
                new VehicleBrand(data.Brand),
                new VehicleModel(data.Model),
                new VehiclePlate(data.Plate),
                new VehicleSize(data.VehicleSize),
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

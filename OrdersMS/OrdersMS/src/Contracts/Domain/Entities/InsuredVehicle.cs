using OrdersMS.Core.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;

namespace OrdersMS.src.Contracts.Domain.Entities
{
    public class InsuredVehicle(
        VehicleId id,
        VehicleBrand brand,
        VehicleModel model,
        VehiclePlate plate,
        VehicleSize size,
        VehicleYear year,
        ClientDNI clientDNI,
        ClientName clientName,
        ClientEmail clientEmail
        ) : Entity<VehicleId>(id)
    {
        private readonly VehicleId _id = id;
        private readonly VehicleBrand _brand = brand;
        private readonly VehicleModel _model = model;
        private readonly VehiclePlate _plate = plate;
        private readonly VehicleSize _vehicleSize = size;
        private readonly VehicleYear _year = year;
        private bool _isActive = true;
        private readonly ClientDNI _clientDNI = clientDNI;
        private readonly ClientName _clientName = clientName;
        private readonly ClientEmail _clientEmail = clientEmail;

        public string GetId() => _id.GetValue();
        public string GetBrand() => _brand.GetValue();
        public string GetModel() => _model.GetValue();
        public string GetPlate() => _plate.GetValue();
        public string GetVehicleSize() => _vehicleSize.GetValue();
        public int GetYear() => _year.GetValue();
        public bool GetIsActive() => _isActive;
        public string GetClientDNI() => _clientDNI.GetValue();
        public string GetClientName() => _clientName.GetValue();
        public string GetClientEmail() => _clientEmail.GetValue();
        public bool SetIsActive(bool isActive) => _isActive = isActive;
    }
}

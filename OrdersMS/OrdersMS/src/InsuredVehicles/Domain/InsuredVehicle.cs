using OrdersMS.Core.Domain.Aggregates;
using OrdersMS.src.InsuredVehicles.Domain.Exceptions;
using OrdersMS.src.InsuredVehicles.Domain.ValueObjects;

namespace OrdersMS.src.InsuredVehicles.Domain
{
    public class InsuredVehicle(VehicleId id) : AggregateRoot<VehicleId>(id)
    {
        private VehicleId _id = id;
        private VehicleBrand _brand;
        private VehicleModel _model;
        private VehiclePlate _plate;
        private VehicleSize _vehicleSize;
        private VehicleYear _year;
        private bool _isActive = true;
        private ClientDNI _clientDni;
        private ClientName _clientName;
        private ClientEmail _clientEmail;

        public string GetId() => _id.GetValue();
        public string GetBrand() => _brand.GetValue();
        public string GetModel() => _model.GetValue();
        public string GetPlate() => _plate.GetValue();
        public string GetVehicleSize() => _vehicleSize.GetValue();
        public int GetYear() => _year.GetValue();
        public bool GetIsActive() => _isActive;
        public string GetClientDNI() => _clientDni.GetValue();
        public string GetClientName() => _clientName.GetValue();
        public string GetClientEmail() => _clientEmail.GetValue();
        public bool SetIsActive(bool isActive) => _isActive = isActive;

        public static InsuredVehicle CreateVehicle(VehicleId id, VehicleBrand brand, VehicleModel model, VehiclePlate plate, VehicleSize vehicleSize, VehicleYear year, ClientDNI clientDni, ClientName clientName, ClientEmail clientEmail)
        {

            var vehicle = new InsuredVehicle(id)
            {
                _brand = brand,
                _model = model,
                _plate = plate,
                _vehicleSize = vehicleSize,
                _year = year,
                _clientDni = clientDni,
                _clientName = clientName,
                _clientEmail = clientEmail
            };
            //vehicle.Apply(VehicleCreated.CreateEvent(id, brand, model, plate, vehicleSize, year, clientDni, clientName, clientEmail));
            return vehicle;
        }

        //public void OnVehicleCreatedEvent(VehicleCreated context)
        //{
        //    _id = new VehicleId(context.Id);
        //    _brand = new VehicleBrand(context.Brand);
        //    _model = new VehicleModel(context.Model);
        //    _plate = new VehiclePlate(context.Plate);
        //    _vehicleSize = Enum.Parse<VehicleSize>(context.VehicleSize);
        //    _year = new VehicleYear(context.Year);
        //    _clientDni = new ClientDNI(context.ClientDNI);
        //    _clientName = new ClientName(context.ClientName);
        //    _clientEmail = new ClientEmail(context.ClientEmail);
        //}

        public override void ValidateState()
        {
            if (_id == null || _brand == null || _model == null || _plate == null || _year == null || _clientDni == null || _clientName == null || _clientEmail == null)
            {
                throw new InvalidVehicleException();
            }
        }
    }
}

using OrdersMS.Core.Domain.Events;
using OrdersMS.src.InsuredVehicles.Domain.ValueObjects;

namespace OrdersMS.src.InsuredVehicles.Domain.Events
{
    public class VehicleCreatedEvent(string dispatcherId, string name, VehicleCreated context) : DomainEvent<object>(dispatcherId, name, context) { }

    public class VehicleCreated(string id, string brand, string model, string plate, string vehicleSize, int year, string clientDNI, string clientName, string clientEmail)
    {
        public readonly string Id = id;
        public readonly string Brand = brand;
        public readonly string Model = model;
        public readonly string Plate = plate;
        public readonly string VehicleSize = vehicleSize;
        public readonly int Year = year;
        public readonly string ClientDNI = clientDNI;
        public readonly string ClientName = clientName;
        public readonly string ClientEmail = clientEmail;

        static public VehicleCreatedEvent CreateEvent(VehicleId id, VehicleBrand brand, VehicleModel model, VehiclePlate plate, VehicleSize vehicleSize, VehicleYear year, ClientDNI clientDNI, ClientName clientName, ClientEmail clientEmail)
        {
            return new VehicleCreatedEvent(
                id.GetValue(),
                typeof(VehicleCreated).Name,
                new VehicleCreated(
                    id.GetValue(),
                    brand.GetValue(),
                    model.GetValue(),
                    plate.GetValue(),
                    vehicleSize.GetValue(),
                    year.GetValue(),
                    clientDNI.GetValue(),
                    clientName.GetValue(),
                    clientEmail.GetValue()
                )
            );
        }
    }
}

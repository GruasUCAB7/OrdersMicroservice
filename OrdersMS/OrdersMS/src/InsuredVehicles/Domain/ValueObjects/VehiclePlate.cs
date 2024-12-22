using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.Core.Utils.RegExps;
using OrdersMS.src.InsuredVehicles.Domain.Exceptions;

namespace OrdersMS.src.InsuredVehicles.Domain.ValueObjects
{
    public class VehiclePlate : IValueObject<VehiclePlate>
    {
        private string Plate { get; }

        public VehiclePlate(string plate)
        {
            if (!PlateRegex.IsMatch(plate))
            {
                throw new InvalidVehiclePlateException();
            }
            Plate = plate;
        }

        public string GetValue()
        {
            return Plate;
        }

        public bool Equals(VehiclePlate other)
        {
            return Plate == other.Plate;
        }
    }
}

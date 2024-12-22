using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.InsuredVehicles.Domain.Exceptions;

namespace OrdersMS.src.InsuredVehicles.Domain.ValueObjects
{
    public class VehicleBrand : IValueObject<VehicleBrand>
    {
        private string Brand { get; }
        public VehicleBrand(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand) || brand.Length < 2)
            {
                throw new InvalidVehicleBrandException();
            }
            Brand = brand;
        }

        public string GetValue()
        {
            return Brand;
        }

        public bool Equals(VehicleBrand other)
        {
            return Brand == other.Brand;
        }
    }
}

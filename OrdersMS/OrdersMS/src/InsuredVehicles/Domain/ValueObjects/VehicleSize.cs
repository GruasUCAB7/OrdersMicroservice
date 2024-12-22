using OrdersMS.src.InsuredVehicles.Domain.Exceptions;

namespace OrdersMS.src.InsuredVehicles.Domain.ValueObjects
{
    public enum VehicleSize
    {
        Ligero,
        Mediano,
        Pesado
    }

    public static class VehicleSizeValue
    {
        public static string GetValue(this VehicleSize vehicleSize)
        {
            if (vehicleSize == VehicleSize.Ligero || vehicleSize == VehicleSize.Mediano || vehicleSize == VehicleSize.Pesado)
            {
                return vehicleSize.ToString();
            }
            throw new InvalidVehicleSizeException();
        }
    }
}

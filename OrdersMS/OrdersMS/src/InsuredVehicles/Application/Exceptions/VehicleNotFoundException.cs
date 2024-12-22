namespace OrdersMS.src.InsuredVehicles.Application.Exceptions
{
    public class VehicleNotFoundException : ApplicationException
    {
        public VehicleNotFoundException() : base("Vehicle not found")
        {
        }
    }
}

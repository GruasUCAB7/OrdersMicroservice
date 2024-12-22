namespace OrdersMS.src.InsuredVehicles.Application.Exceptions
{
    public class VehicleUpdateFailedException : ApplicationException
    {
        public VehicleUpdateFailedException() : base("The vehicle could not be updated correctly")
        {
        }
    }
}

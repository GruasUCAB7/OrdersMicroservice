namespace OrdersMS.src.Contracts.Application.Exceptions
{
    public class VehicleNotAvailableException : ApplicationException
    {
        public VehicleNotAvailableException() : base("Vehicle not available") { }
    }
}

namespace OrdersMS.src.Contracts.Application.Exceptions
{
    public class VehicleNotFoundException : ApplicationException
    {
        public VehicleNotFoundException() : base("Vehicle not found")
        {
        }
    }
}

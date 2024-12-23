using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.InsuredVehicles.Domain.Exceptions
{
    public class InvalidVehicleBrandException : DomainException
    {
        public InvalidVehicleBrandException() : base("Invalid vehicle brand")
        {
        }
    }
}

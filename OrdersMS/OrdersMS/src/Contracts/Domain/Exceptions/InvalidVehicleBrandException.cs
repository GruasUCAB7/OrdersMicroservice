using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidVehicleBrandException : DomainException
    {
        public InvalidVehicleBrandException() : base("Invalid vehicle brand")
        {
        }
    }
}

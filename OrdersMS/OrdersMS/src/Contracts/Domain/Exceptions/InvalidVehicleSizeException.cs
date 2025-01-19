using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidVehicleSizeException : DomainException
    {
        public InvalidVehicleSizeException() : base("Invalid vehicle size")
        {
        }
    }
}

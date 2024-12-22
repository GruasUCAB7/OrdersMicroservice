using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidVehicleIdException : DomainException
    {
        public InvalidVehicleIdException() : base("Invalid vehicle ID")
        {
        }
    }
}

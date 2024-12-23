using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidVehicleModelException : DomainException
    {
        public InvalidVehicleModelException() : base("Invalid vehicle model")
        {
        }
    }
}

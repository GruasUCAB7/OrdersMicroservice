using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidVehiclePlateException : DomainException
    {
        public InvalidVehiclePlateException() : base("Invalid vehicle plate")
        {
        }
    }
}

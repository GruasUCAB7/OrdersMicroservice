using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.InsuredVehicles.Domain.Exceptions
{
    public class InvalidVehiclePlateException : DomainException
    {
        public InvalidVehiclePlateException() : base("Invalid vehicle plate")
        {
        }
    }
}

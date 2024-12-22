using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.InsuredVehicles.Domain.Exceptions
{
    public class InvalidVehicleException : DomainException
    {
        public InvalidVehicleException() : base("Invalid vehicle")
        {
        }
    }
}

using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.InsuredVehicles.Domain.Exceptions
{
    public class InvalidVehicleSizeException : DomainException
    {
        public InvalidVehicleSizeException() : base("Invalid vehicle size")
        {
        }
    }
}

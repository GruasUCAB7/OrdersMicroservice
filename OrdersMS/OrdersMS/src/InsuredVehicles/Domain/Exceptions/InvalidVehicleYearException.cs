using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.InsuredVehicles.Domain.Exceptions
{
    public class InvalidVehicleYearException : DomainException
    {
        public InvalidVehicleYearException() : base("Invalid vehicle year")
        {
        }
    }
}

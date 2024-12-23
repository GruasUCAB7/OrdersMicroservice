using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.InsuredVehicles.Domain.Exceptions
{
    public class InvalidClientDNIException : DomainException
    {
        public InvalidClientDNIException() : base("Invalid client DNI")
        {
        }
    }
}

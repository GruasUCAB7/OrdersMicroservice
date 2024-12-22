using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidClientDNIException : DomainException
    {
        public InvalidClientDNIException() : base("Invalid client DNI")
        {
        }
    }
}

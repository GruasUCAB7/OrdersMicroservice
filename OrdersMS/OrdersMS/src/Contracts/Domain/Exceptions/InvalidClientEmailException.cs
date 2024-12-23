using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidClientEmailException : DomainException
    {
        public InvalidClientEmailException() : base("Invalid client email")
        {
        }
    }
}

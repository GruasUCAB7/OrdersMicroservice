using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.Exceptions
{
    public class InvalidExtraCostIdException : DomainException
    {
        public InvalidExtraCostIdException() : base("Invalid extra cost ID")
        {
        }
    }
}

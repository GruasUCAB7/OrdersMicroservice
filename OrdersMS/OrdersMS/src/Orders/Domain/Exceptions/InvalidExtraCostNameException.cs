using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.Exceptions
{
    public class InvalidExtraCostNameException : DomainException
    {
        public InvalidExtraCostNameException() : base("Invalid extra cost name")
        {
        }
    }
}

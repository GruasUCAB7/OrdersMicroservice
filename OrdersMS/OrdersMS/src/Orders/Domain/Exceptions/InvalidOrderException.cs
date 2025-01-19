using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.Exceptions
{
    public class InvalidOrderException : DomainException
    {
        public InvalidOrderException() : base("Invalid order")
        {
        }
    }
}

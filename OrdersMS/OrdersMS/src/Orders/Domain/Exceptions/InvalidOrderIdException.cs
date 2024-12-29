using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.Exceptions
{
    public class InvalidOrderIdException : DomainException
    {
        public InvalidOrderIdException() : base("Invalid order ID")
        {
        }
    }
}

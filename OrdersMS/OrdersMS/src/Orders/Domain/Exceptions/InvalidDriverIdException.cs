using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.Exceptions
{
    public class InvalidDriverIdException : DomainException
    {
        public InvalidDriverIdException() : base("Invalid driver ID")
        {
        }
    }
}

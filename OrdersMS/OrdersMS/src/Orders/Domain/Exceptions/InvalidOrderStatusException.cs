using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.Exceptions
{
    public class InvalidOrderStatusException : DomainException
    {
        public InvalidOrderStatusException(string status) : base(status)
        {
        }
    }
}

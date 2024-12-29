using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.Exceptions
{
    public class InvalidCoordinatesException(string message) : DomainException(message)
    {
    }
}

using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.Exceptions
{
    public class InvalidIncidentDateException() : DomainException($"Invalid incident date")
    {
    }
}

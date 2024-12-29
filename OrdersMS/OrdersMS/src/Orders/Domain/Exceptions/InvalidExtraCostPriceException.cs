using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.Exceptions
{
    public class InvalidExtraCostPriceException(string message) : DomainException(message)
    {
    }
}

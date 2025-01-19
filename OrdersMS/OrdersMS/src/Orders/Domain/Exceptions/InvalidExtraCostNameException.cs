using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.Exceptions
{
    public class InvalidExtraCostNameException(string name) : DomainException($"Invalid extra cost name: {name}")
    {
    }
}

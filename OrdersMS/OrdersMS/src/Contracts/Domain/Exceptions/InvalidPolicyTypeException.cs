using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidPolicyTypeException(string message) : DomainException(message)
    {
    }
}

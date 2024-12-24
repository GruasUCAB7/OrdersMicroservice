using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidContractStatusException : DomainException
    {
        public InvalidContractStatusException(string message) : base(message)
        {
        }
    }
}

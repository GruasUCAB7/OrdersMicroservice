using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidContractException : DomainException
    {
        public InvalidContractException() : base("Invalid contract")
        {
        }
    }
}

namespace OrdersMS.src.Contracts.Application.Exceptions
{
    public class ContractNotAvailableException : ApplicationException
    {
        public ContractNotAvailableException() : base("Contract is not active") { }
    }
}

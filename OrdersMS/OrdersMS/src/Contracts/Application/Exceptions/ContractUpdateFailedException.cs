namespace OrdersMS.src.Contracts.Application.Exceptions
{
    public class ContractUpdateFailedException : ApplicationException
    {
        public ContractUpdateFailedException() : base("The contract could not be updated correctly")
        {
        }
    }
}

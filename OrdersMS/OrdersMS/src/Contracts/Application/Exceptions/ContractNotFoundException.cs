namespace OrdersMS.src.Contracts.Application.Exceptions
{
    public class ContractNotFoundException : ApplicationException
    {
        public ContractNotFoundException() : base("Contract not found")
        {
        }
    }
}

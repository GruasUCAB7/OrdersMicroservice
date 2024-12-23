namespace OrdersMS.src.Contracts.Application.Exceptions
{
    public class PolicyNotFoundException : ApplicationException
    {
        public PolicyNotFoundException() : base("Insurance policy not found")
        {
        }
    }
}

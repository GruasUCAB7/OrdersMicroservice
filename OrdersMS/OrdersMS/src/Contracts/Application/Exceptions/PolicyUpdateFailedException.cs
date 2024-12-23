namespace OrdersMS.src.Contracts.Application.Exceptions
{
    public class PolicyUpdateFailedException : ApplicationException
    {
        public PolicyUpdateFailedException() : base("The insurance policy could not be updated correctly")
        {
        }
    }
}

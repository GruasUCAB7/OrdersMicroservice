namespace OrdersMS.src.Orders.Application.Exceptions
{
    public class ExtraServicesAppliedUpdateFailedException : ApplicationException
    {
        public ExtraServicesAppliedUpdateFailedException() : base("Extra services applied could not be updated correctly")
        {
        }
    }
}

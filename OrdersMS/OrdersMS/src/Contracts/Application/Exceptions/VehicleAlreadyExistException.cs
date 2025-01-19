namespace OrdersMS.src.Contracts.Application.Exceptions
{
    public class VehicleAlreadyExistException : ApplicationException
    {
        public VehicleAlreadyExistException(string plate)
            : base($"Vehicle with plate {plate} already exist")
        {
        }
    }
}

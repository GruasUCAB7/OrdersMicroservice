namespace OrdersMS.src.Orders.Infrastructure.Types
{
    public class DriverResponse
    {
        public string id { get; set; }
    }

    public class AvailableDriverResponse
    {
        public string id { get; set; }
        public string craneAssigned { get; set; }
    }
}

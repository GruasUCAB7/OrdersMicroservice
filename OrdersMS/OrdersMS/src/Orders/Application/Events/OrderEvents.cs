namespace OrdersMS.src.Orders.Application.Events
{
    public class OrderCreatedEvent(Guid orderId)
    {
        public Guid OrderId { get; } = orderId;
    }

    public class DriverAssignedToOrderEvent(Guid orderId)
    {
        public Guid OrderId { get; } = orderId;
    }

    public class DriverAcceptedOrderEvent(Guid orderId)
    {
        public Guid OrderId { get; } = orderId;
    }

    public class DriverRefusedOrderEvent(Guid orderId)
    {
        public Guid OrderId { get; } = orderId;
    }

    public class DriverIsAtTheIncidentEvent(Guid orderId)
    {
        public Guid OrderId { get; } = orderId;
    }

    public class OrderInProcessEvent(Guid orderId)
    {
        public Guid OrderId { get; } = orderId;
    }

    public class OrderCompletedEvent(Guid orderId)
    {
        public Guid OrderId { get; } = orderId;
    }

    public class OrderPaidEvent(Guid orderId)
    {
        public Guid OrderId { get; } = orderId;
    }

    public class OrderCanceledEvent(Guid orderId)
    {
        public Guid OrderId { get; } = orderId;
    }
}

using MassTransit;

namespace OrdersMS.src.Orders.Application.SagaData
{
    public class OrderStatusSagaData : SagaStateMachineInstance, ISagaVersion
    {
        public Guid CorrelationId { get; set; }
        public string? CurrentState { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public int Version { get; set; }
    }
}

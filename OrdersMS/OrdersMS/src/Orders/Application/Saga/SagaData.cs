using MassTransit;

namespace OrdersMS.src.Orders.Application.Saga
{
    public class SagaData : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }

        public Guid SubscriberId { get; set; }
        public bool OrderCreated { get; set; }
        public bool PorAsignarStatus { get; set; }
        public bool PorAceptarStatus { get; set; }
        public bool AceptadoStatus { get; set; }
        public bool LocalizadoStatus { get; set; }
        public bool EnProcesoStatus { get; set; }
        public bool FinalizadoStatus { get; set; }
        public bool PagadoStatus { get; set; }
        public bool CanceladaStatus { get; set; }
    }
}

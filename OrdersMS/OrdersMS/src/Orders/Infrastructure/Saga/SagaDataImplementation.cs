using MassTransit;
using OrdersMS.src.Orders.Application.Saga;
using OrdersMS.src.Orders.Infrastructure.EventsMassTransit;

namespace OrdersMS.src.Orders.Infrastructure.Saga
{
    public class SagaDataImplementation : MassTransitStateMachine<SagaData>
    {
        public State PorAsignar { get; set; }
        public State PorAceptar { get; set; }
        public State Aceptado { get; set; }
        public State Localizado { get; set; }
        public State EnProceso { get; set; }
        public State Finalizado { get; set; }
        public State Pagado { get; set; }
        public State Cancelada { get; set; }

        public Event<OrderCreated> OrderCreated { get; set; }
        public Event<PorAsignarStatus> PorAsignarStatus { get; set; }
        public Event<PorAceptarStatus> PorAceptarStatus { get; set; }
        public Event<AceptadoStatus> AceptadoStatus { get; set; }
        public Event<LocalizadoStatus> LocalizadoStatus { get; set; }
        public Event<EnProcesoStatus> EnProcesoStatus { get; set; }
        public Event<FinalizadoStatus> FinalizadoStatus { get; set; }
        public Event<CanceladaStatus> CanceladaStatus { get; set; }
        public Event<PagadoStatus> PagadoStatus { get; set; }

        public SagaDataImplementation()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderCreated, e => e.CorrelateById(m => m.Message.SubdcriberId));
            Event(() => PorAsignarStatus, e => e.CorrelateById(m => m.Message.SubdcriberId));
            Event(() => PorAceptarStatus, e => e.CorrelateById(m => m.Message.SubdcriberId));
            Event(() => AceptadoStatus, e => e.CorrelateById(m => m.Message.SubdcriberId));
            Event(() => LocalizadoStatus, e => e.CorrelateById(m => m.Message.SubdcriberId));
            Event(() => EnProcesoStatus, e => e.CorrelateById(m => m.Message.SubdcriberId));
            Event(() => FinalizadoStatus, e => e.CorrelateById(m => m.Message.SubdcriberId));
            Event(() => CanceladaStatus, e => e.CorrelateById(m => m.Message.SubdcriberId));
            //Event(() => PagadoStatus, e => e.CorrelateById(m => m.Message.SubdcriberId));

            Initially(
            When(PorAsignarStatus)
                .Then(context =>
                {
                    context.Saga.SubscriberId = context.Message.SubdcriberId;
                }));
                //.TransitionTo(PorAceptar)
                //.Publish(context => new PorAceptarStatus(context.Message.SubdcriberId)));
        }

        
    }
}

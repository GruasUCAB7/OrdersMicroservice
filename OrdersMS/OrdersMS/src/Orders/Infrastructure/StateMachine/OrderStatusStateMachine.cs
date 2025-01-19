using MassTransit;
using OrdersMS.src.Orders.Application.Events;
using OrdersMS.src.Orders.Application.SagaData;

namespace OrdersMS.src.Orders.Infrastructure.StateMachine
{
    public class OrderStatusStateMachine : MassTransitStateMachine<OrderStatusSagaData>
    {
        public State PorAsignar { get; set; }
        public State PorAceptar { get; set; }
        public State Aceptado { get; set; }
        public State Localizado { get; set; }
        public State EnProceso { get; set; }
        public State Finalizado { get; set; }
        public State Pagado { get; set; }
        public State Cancelada { get; set; }


        public Event<OrderCreatedEvent> OrderCreated { get; set; }
        public Event<DriverAssignedToOrderEvent> DriverAssignedToOrder { get; set; }
        public Event<DriverAcceptedOrderEvent> DriverAcceptedOrder { get; set; }
        public Event<DriverRefusedOrderEvent> DriverRefusedOrder { get; set; }
        public Event<DriverIsAtTheIncidentEvent> DriverIsAtTheIncident { get; set; }
        public Event<OrderInProcessEvent> OrderInProcess { get; set; }
        public Event<OrderCompletedEvent> OrderCompleted { get; set; }
        public Event<OrderPaidEvent> OrderPaid { get; set; }
        public Event<OrderCanceledEvent> OrderCanceled { get; set; }
        
        
        public OrderStatusStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderCreated, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => DriverAssignedToOrder, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => DriverAcceptedOrder, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => DriverRefusedOrder, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => DriverIsAtTheIncident, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderInProcess, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderCompleted, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderPaid, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderCanceled, x => x.CorrelateById(context => context.Message.OrderId));


            Initially(
                When(OrderCreated)
                    .Then(context =>
                    {
                        context.Saga.CorrelationId = context.Message.OrderId;
                        context.Saga.CreatedAt = DateTime.UtcNow;
                    })
                    .TransitionTo(PorAsignar));

            During(PorAsignar,
                When(DriverAssignedToOrder)
                    .Then(context =>
                    {
                        context.Saga.LastUpdated = DateTime.UtcNow;
                    })
                    .TransitionTo(PorAceptar));

            During(PorAceptar,
                When(DriverAcceptedOrder)
                    .Then(context =>
                    {
                        context.Saga.LastUpdated = DateTime.UtcNow;
                    })
                    .TransitionTo(Aceptado));


            During(PorAceptar,
                When(DriverRefusedOrder)
                    .Then(context =>
                    {
                        context.Saga.LastUpdated = DateTime.UtcNow;
                    })
                    .TransitionTo(PorAsignar));

            During(Aceptado,
               When(DriverIsAtTheIncident)
                   .Then(context =>
                   {
                       context.Saga.LastUpdated = DateTime.UtcNow;
                   })
                   .TransitionTo(Localizado));

            During(Localizado,
               When(OrderInProcess)
                   .Then(context =>
                   {
                       context.Saga.LastUpdated = DateTime.UtcNow;
                   })
                   .TransitionTo(EnProceso));

            During(EnProceso,
               When(OrderCompleted
)
                   .Then(context =>
                   {
                       context.Saga.LastUpdated = DateTime.UtcNow;
                   })
                   .TransitionTo(Finalizado));

            During(Finalizado,
               When(OrderPaid)
                   .Then(context =>
                   {
                       context.Saga.LastUpdated = DateTime.UtcNow;
                   })
                   .TransitionTo(Pagado));

            During(Localizado,
               When(OrderCanceled)
                   .Then(context =>
                   {
                       context.Saga.LastUpdated = DateTime.UtcNow;
                   })
                   .TransitionTo(Cancelada));

            During(EnProceso,
               When(OrderCanceled)
                   .Then(context =>
                   {
                       context.Saga.LastUpdated = DateTime.UtcNow;
                   })
                   .TransitionTo(Cancelada));
        }
    }
}

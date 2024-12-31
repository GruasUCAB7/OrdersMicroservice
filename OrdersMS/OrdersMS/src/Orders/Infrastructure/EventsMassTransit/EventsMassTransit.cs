namespace OrdersMS.src.Orders.Infrastructure.EventsMassTransit
{
    public class OrderCreated
    {
        public Guid SubdcriberId { get; init; }
    }

    public class PorAsignarStatus
    {
        public Guid SubdcriberId { get; init; }
    }

    public class PorAceptarStatus
    {
        public Guid SubdcriberId { get; init; }
    }
    public class AceptadoStatus
    {
        public Guid SubdcriberId { get; init; }
    }

    public class LocalizadoStatus
    {
        public Guid SubdcriberId { get; init; }
    }

    public class EnProcesoStatus
    {
        public Guid SubdcriberId { get; init; }
    }

    public class FinalizadoStatus
    {
        public Guid SubdcriberId { get; init; }
    }

    public class CanceladaStatus
    {
        public Guid SubdcriberId { get; init; }
    }

    public class PagadoStatus
    {
        public Guid SubscriberId { get; init; }
    }
}

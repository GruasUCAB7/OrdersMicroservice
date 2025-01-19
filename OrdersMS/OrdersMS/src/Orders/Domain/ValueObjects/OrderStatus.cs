using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Orders.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.ValueObjects
{
    public class OrderStatus : IValueObject<OrderStatus>
    {
        public static readonly string PorAsignar = "Por Asignar";
        public static readonly string PorAceptar = "Por Aceptar";
        public static readonly string Aceptado = "Aceptado";
        public static readonly string Localizado = "Localizado";
        public static readonly string EnProceso = "En Proceso";
        public static readonly string Finalizado = "Finalizado";
        public static readonly string Cancelada = "Cancelada";
        public static readonly string Pagado = "Pagado";

        public string Status { get; }

        public OrderStatus(string status)
        {
            if (status != PorAsignar
                && status != PorAceptar
                && status != Aceptado
                && status != Localizado
                && status != EnProceso
                && status != Finalizado
                && status != Cancelada
                && status != Pagado)
            {
                throw new InvalidOrderStatusException($"Invalid order status: {status}. Allowed values are: {PorAsignar}, {PorAceptar}, {Aceptado}, {Localizado}, {EnProceso}, {Finalizado}, {Cancelada}, {Pagado}.");
            }
            Status = status;
        }

        public string GetValue()
        {
            return Status;
        }

        public bool Equals(OrderStatus other)
        {
            return Status == other.Status;
        }
    }
}

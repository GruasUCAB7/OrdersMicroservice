using OrdersMS.Core.Domain.Events;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Domain.Events
{
    public class ExtraCostAddedEvent(string dispatcherId, string name, ExtraCostAdded context) : DomainEvent<object>(dispatcherId, name, context) { }
    public class ExtraCostAdded(string Id, string Name, decimal Price)
    {
        public readonly string id = Id;
        public readonly string name = Name;
        public readonly decimal price = Price;

        public static ExtraCostAddedEvent CreateEvent(
            OrderId OrderId,
            ExtraCostId Id,
            ExtraCostName Name,
            ExtraCostPrice Price)
        {
            return new ExtraCostAddedEvent( 
                OrderId.GetValue(),
                typeof(ExtraCostAdded).Name,
                new ExtraCostAdded(
                    Id.GetValue(),
                    Name.GetValue(),
                    Price.GetValue()
                )
            );
        }
    }
}

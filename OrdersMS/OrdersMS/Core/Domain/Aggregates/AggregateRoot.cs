using System.Reflection;
using OrdersMS.Core.Domain.Entities;
using OrdersMS.Core.Domain.Events;
using OrdersMS.Core.Domain.ValueObjects;


namespace OrdersMS.Core.Domain.Aggregates
{
    public abstract class AggregateRoot<T>(T id) : Entity<T>(id) where T : class, IValueObject<T>
    {
        private readonly List<DomainEvent<object>> _events = [];

        public List<DomainEvent<object>> PullEvents()
        {
            var events = new List<DomainEvent<object>>(_events);
            _events.Clear();
            return events;
        }

        public void Apply(DomainEvent<object> Event)
        {
            MethodInfo? Handler = GetEventHandler(Event) ?? throw new Exception($"Handler not found for event: {Event.GetType().Name}");
            _events.Add(Event);
            Handler.Invoke(this, new object[] { Event });
            ValidateState();
        }

        private protected MethodInfo? GetEventHandler(DomainEvent<object> Event)
        {
            MethodInfo? Handler = GetType().GetMethod(
                $"On{Event.GetType().Name}",
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);
            return Handler;
        }

        public abstract void ValidateState();
    }
}
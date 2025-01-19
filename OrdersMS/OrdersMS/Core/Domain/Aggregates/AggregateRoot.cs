using System.Reflection;
using OrdersMS.Core.Domain.Entities;
using OrdersMS.Core.Domain.Events;
using OrdersMS.Core.Domain.ValueObjects;


namespace OrdersMS.Core.Domain.Aggregates
{
    public abstract class AggregateRoot<T>(T id) : Entity<T>(id) where T : class, IValueObject<T>
    {
        private readonly List<DomainEvent<object>> Events = [];

        protected new T Id { get; private set; } = id;

        public List<DomainEvent<object>> PullEvents()
        {
            var events = new List<DomainEvent<object>>(Events);
            Events.Clear();
            return events;
        }

        public void Apply(DomainEvent<object> @event)
        {
            var handler = GetEventHandler(@event) ?? throw new Exception($"Handler not found for event: {@event.GetType().Name}");
            handler.Invoke(this, new object[] { @event.Context });
            ValidateState();
            Events.Add(@event);
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
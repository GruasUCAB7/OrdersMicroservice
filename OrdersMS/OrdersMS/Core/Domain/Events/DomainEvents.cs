namespace OrdersMS.Core.Domain.Events
{
    public class DomainEvent<T>(string dispatcherId, string name, T context)
    {
        public string DispatcherId { get; } = dispatcherId;
        public string Name { get; } = name;

        public DateTime Timestamp = DateTime.UtcNow;
        public T Context { get; } = context;
    }
}

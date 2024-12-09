using OrdersMS.Core.Domain.ValueObjects;

namespace OrdersMS.Core.Domain.Entities
{
    public abstract class Entity<T>(T id) where T : IValueObject<T>
    {
        protected readonly T _id = id;

        public T Id => _id;

        public bool Equals(Entity<T> id)
        {
            return _id.Equals(id);
        }
    }
}

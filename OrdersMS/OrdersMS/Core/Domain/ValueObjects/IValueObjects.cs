namespace OrdersMS.Core.Domain.ValueObjects
{
    public interface IValueObject<T>
{
    bool Equals(T other);
}
}
namespace OrdersMS.Core.Utils.Optional
{
    public class Optional<T>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        private Optional(T value, bool hasValue)
        {
            _value = value;
            _hasValue = hasValue;
        }

        public T Unwrap()
        {
            if (!_hasValue) throw new InvalidOperationException("Value is not present");
            return _value;
        }

        public bool HasValue => _hasValue;

        public static Optional<T> Of(T value)
        {
            return new Optional<T>(value, true);
        }

        public static Optional<T> Empty()
        {
            return new Optional<T>(default!, false);
        }
    }
}

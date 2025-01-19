namespace OrdersMS.Core.Utils.Result
{
    public class Result<T>
    {
        private readonly T _value;
        private readonly Exception? _error;

        private Result(T value, Exception? error)
        {
            if (value != null && error != null)
                throw new InvalidOperationException("Value and error cannot be defined at the same time");

            _value = value;
            _error = error;
        }

        public T Unwrap()
        {
            if (_error != null) throw _error;
            return _value;
        }

        public bool IsSuccessful => _error == null;

        public bool IsFailure => _error != null;

        public string ErrorMessage
        {
            get
            {
                if (_error == null) throw new InvalidOperationException("No error in result");
                return _error.Message;
            }
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(value, null);
        }

        public static Result<T> Failure(Exception error)
        {
            return new Result<T>(default!, error);
        }
    }
}

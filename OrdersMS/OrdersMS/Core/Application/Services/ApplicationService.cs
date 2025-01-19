using OrdersMS.Core.Utils.Result;

namespace OrdersMS.Core.Application.Services
{
    public interface IService<T, R>
    {
        Task<Result<R>> Execute(T data);
    }
}

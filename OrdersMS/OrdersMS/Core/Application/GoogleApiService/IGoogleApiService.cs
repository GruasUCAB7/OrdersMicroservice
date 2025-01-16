using OrdersMS.Core.Infrastructure.GoogleMaps;
using OrdersMS.Core.Utils.Result;

namespace OrdersMS.Core.Application.GoogleApiService
{
    public interface IGoogleApiService
    {
        Task<Result<Coordinates>> GetCoordinatesFromAddress(string address);
    }
}

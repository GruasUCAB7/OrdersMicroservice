using OrdersMS.Core.Infrastructure.GoogleMaps;

namespace OrdersMS.Core.Application.GoogleApiService
{
    public interface IGoogleApiService
    {
        //Task<List<VehicleAvailableDto>> GetDistanceAvailableVehiclesToOrigin(List<VehicleAvailableDto> listVehicleAvailableDto, double originLatitude, double originLongitude);

        //Task<CraneServiceDto> GetDistanceCompleteRoute(CraneServiceDto craneServiceDto, double originLatitude, double originLongitude);

        //Task<VehicleDto?> GetDistanceVehicleToOrigin(VehicleDto? vehicleDto, double originLatitude, double originLongitude);

        Task<Coordinates> GetCoordinatesFromAddress(string address);
    }
}

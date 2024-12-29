using System.Net;
//using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
using OrdersMS.Core.Application.GoogleApiService;
using RestSharp;

namespace OrdersMS.Core.Infrastructure.GoogleMaps
{
    public class GoogleApiService(
        ILogger<GoogleApiService> logger,
        IRestClient client
        ) : IGoogleApiService
    {
        private readonly ILogger<GoogleApiService> _logger = logger;
        private readonly string _apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? string.Empty;
        private readonly string _mapsUrl = Environment.GetEnvironmentVariable("GOOGLE_MAPS_BASE_URL") ?? "https://maps.googleapis.com/maps/api/";
        private readonly IRestClient _client = client;

        public async Task<Coordinates> GetCoordinatesFromAddress(string address)
        {
            var encodedAddress = Uri.EscapeDataString(address);
            var requestUrl = $"{_mapsUrl}geocode/json?address={encodedAddress}&key={_apiKey}";

            var request = new RestRequest(requestUrl, Method.Get);
            var response = await _client.ExecuteAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(
                    "Error on "
                    + requestUrl
                    + " Status Code: "
                    + response.StatusCode
                    + " Content: "
                    + response.Content);

            var content = response.Content;
            if (string.IsNullOrEmpty(content))
                throw new Exception("Response content is null or empty.");

            var geocodeResponse = JsonSerializer.Deserialize<GeocodeResponse>(content);
            if (geocodeResponse == null || geocodeResponse.Results.Length == 0)
                throw new Exception("Failed to get coordinates from address.");

            var location = geocodeResponse.Results[0].Geometry.Location;
            return new Coordinates { Latitude = location.Lat, Longitude = location.Lng };
            
        }

        //public async Task<List<VehicleAvailableDto>> GetDistanceAvailableVehiclesToOrigin(
        //    List<VehicleAvailableDto> listVehicleAvailableDto,
        //    double originLatitude,
        //    double originLongitude
        //)
        //{
        //    try
        //    {
        //        var vehiclesLocationBuilder = new StringBuilder();
        //        foreach (var item in listVehicleAvailableDto)
        //        {
        //            if (vehiclesLocationBuilder.Length > 0)
        //            {
        //                vehiclesLocationBuilder.Append('|');
        //            }
        //            vehiclesLocationBuilder.AppendFormat("{0},{1}", item.Latitude, item.Longitude);
        //        }

        //        var vehiclesLocation = vehiclesLocationBuilder.ToString();
        //        var orgLocation = $"{originLatitude},{originLongitude}";

        //        var requestUrl = $"{_mapsUrl}distancematrix/json?origins={vehiclesLocation}&destinations={orgLocation}&key={_apiKey}";
        //        var request = new RestRequest(requestUrl, Method.Get);
        //        var response = await _client.ExecuteAsync(request);
        //        if (response.StatusCode != HttpStatusCode.OK)
        //            throw new GoogleApiException(
        //                "Error on "
        //                + requestUrl
        //                + " Status Code: "
        //                + response.StatusCode
        //                + " Content: "
        //                + response.Content);

        //        var content = response.Content;
        //        var distanceMatrix = JsonSerializer.Deserialize<DistanceMatrixResponse>(content);
        //        for (var i = 0; i < listVehicleAvailableDto.Count; i++)
        //        {
        //            var row = distanceMatrix!.Rows[i];
        //            if (row.Elements[0].Status == "OK")
        //            {
        //                listVehicleAvailableDto[i].Distance = row.Elements[0].Distance.Text;
        //                listVehicleAvailableDto[i].DistanceValue = row.Elements[0].Distance.Value;
        //                listVehicleAvailableDto[i].Eta = row.Elements[0].Duration.Text;
        //                listVehicleAvailableDto[i].EtaValue = row.Elements[0].Duration.Value;
        //            }
        //            else
        //                _logger.LogWarning(
        //                    "There is an issue when trying assign distances with google map api, the status of the distance is: {Status}",
        //                    row.Elements[0].Status
        //                );
        //        }

        //        return listVehicleAvailableDto;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(
        //            ex,
        //            "Error when trying to calculate distances with google map api. {Message}",
        //            ex.Message
        //        );
        //        throw;
        //    }
        //}

        //public async Task<CraneServiceDto> GetDistanceCompleteRoute(
        //    CraneServiceDto craneServiceDto,
        //    double originLatitude,
        //    double originLongitude
        //)
        //{
        //    try
        //    {
        //        var origin = $"{originLatitude},{originLongitude}";
        //        var destination =
        //            $"{craneServiceDto.LatitudeDestination},{craneServiceDto.LongitudeDestination}";

        //        var requestUrl = $"{_mapsUrl}distancematrix/json?origins={origin}&destinations={destination}&key={_apiKey}";
        //        var request = new RestRequest(requestUrl, Method.Get);
        //        var response = await _client.ExecuteAsync(request);

        //        if (response.StatusCode != HttpStatusCode.OK)
        //            throw new Exception(
        //                "Error on "
        //                + requestUrl
        //                + " Status Code: "
        //                + response.StatusCode
        //                + " Content: "
        //                + response.Content);

        //        var content = response.Content;
        //        var distanceMatrix = JsonSerializer.Deserialize<DistanceMatrixResponse>(content);
        //        if (distanceMatrix == null)
        //            throw new Exception("Failed to deserialize distance matrix response.");

        //        var row = distanceMatrix.Rows[0];
        //        if (row.Elements[0].Status == "OK")
        //        {
        //            craneServiceDto.Distance = row.Elements[0].Distance.Text;
        //            craneServiceDto.DistanceValue = row.Elements[0].Distance.Value;
        //            craneServiceDto.Eta = row.Elements[0].Duration.Text;
        //        }
        //        else
        //            _logger.LogWarning(
        //                "There is an issue when trying assign the distance for the crane service with google map api, the status of the distance is: {Status}",
        //                row.Elements[0].Status
        //            );

        //        return craneServiceDto;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(
        //            ex,
        //            "Error when trying to calculate distance for the crane service with google map api. {Message}",
        //            ex.Message
        //        );
        //        throw;
        //    }
        //}

        //public async Task<VehicleDto?> GetDistanceVehicleToOrigin(
        //    VehicleDto? vehicleDto,
        //    double originLatitude,
        //    double originLongitude
        //)
        //{
        //    try
        //    {
        //        var origin = $"{originLatitude},{originLongitude}";
        //        var destination = $"{vehicleDto!.Latitude},{vehicleDto.Longitude}";

        //        var requestUrl = $"{_mapsUrl}distancematrix/json?origins={origin}&destinations={destination}&key={_apiKey}";
        //        var request = new RestRequest(requestUrl, Method.Get);
        //        var response = await _client.ExecuteAsync(request);

        //        if (response.StatusCode != HttpStatusCode.OK)
        //            throw new Exception(
        //                "Error on "
        //                + requestUrl
        //                + " Status Code: "
        //                + response.StatusCode
        //                + " Content: "
        //                + response.Content);

        //        var content = response.Content;
        //        var distanceMatrix = JsonSerializer.Deserialize<DistanceMatrixResponse>(content);
        //        if (distanceMatrix == null)
        //            throw new Exception("Failed to deserialize distance matrix response.");

        //        var row = distanceMatrix.Rows[0];
        //        if (row.Elements[0].Status == "OK")
        //        {
        //            vehicleDto.Distance = row.Elements[0].Distance.Text;
        //            vehicleDto.DistanceValue = row.Elements[0].Distance.Value;
        //            vehicleDto.Eta = row.Elements[0].Duration.Text;
        //        }
        //        else
        //            _logger.LogWarning(
        //                "There is an issue when trying assign the vehicle distance to the origin with google map api, the status of the distance is: {Status}",
        //                row.Elements[0].Status
        //            );

        //        return vehicleDto;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(
        //            ex,
        //            "Error when trying to calculate distance for the vehicle distance to the origin with google map api. {Message}",
        //            ex.Message
        //        );
        //        throw;
        //    }
        //}
    }

    public class Coordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }


    public class GeocodeResponse
    {
        [JsonPropertyName("results")]
        public GeocodeResult[] Results { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class GeocodeResult
    {
        [JsonPropertyName("address_components")]
        public AddressComponent[] AddressComponents { get; set; }

        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonPropertyName("geometry")]
        public Geometry Geometry { get; set; }

        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; }

        [JsonPropertyName("types")]
        public string[] Types { get; set; }
    }

    public class AddressComponent
    {
        [JsonPropertyName("long_name")]
        public string LongName { get; set; }

        [JsonPropertyName("short_name")]
        public string ShortName { get; set; }

        [JsonPropertyName("types")]
        public string[] Types { get; set; }
    }

    public class Geometry
    {
        [JsonPropertyName("bounds")]
        public Bounds Bounds { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; }

        [JsonPropertyName("location_type")]
        public string LocationType { get; set; }

        [JsonPropertyName("viewport")]
        public Viewport Viewport { get; set; }
    }

    public class Bounds
    {
        [JsonPropertyName("northeast")]
        public Location Northeast { get; set; }

        [JsonPropertyName("southwest")]
        public Location Southwest { get; set; }
    }

    public class Location
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }

    public class Viewport
    {
        [JsonPropertyName("northeast")]
        public Location Northeast { get; set; }

        [JsonPropertyName("southwest")]
        public Location Southwest { get; set; }
    }
}

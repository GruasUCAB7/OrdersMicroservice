using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using OrdersMS.Core.Application.GoogleApiService;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Exceptions;
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

        public async Task<Result<Coordinates>> GetCoordinatesFromAddress(string address)
        {
            var encodedAddress = Uri.EscapeDataString(address);
            var requestUrl = $"{_mapsUrl}geocode/json?address={encodedAddress}&key={_apiKey}";

            var request = new RestRequest(requestUrl, Method.Get);
            var response = await _client.ExecuteAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
                return Result<Coordinates>.Failure(new CoordinatesNullException(
                    "Error on "
                    + requestUrl
                    + " Status Code: "
                    + response.StatusCode
                    + " Content: "
                    + response.Content));

            var content = response.Content;
            if (string.IsNullOrEmpty(content))
                return Result<Coordinates>.Failure(new CoordinatesNullException("Response content is null or empty."));

            var geocodeResponse = JsonSerializer.Deserialize<GeocodeResponse>(content);
            if (geocodeResponse == null || geocodeResponse.Results.Length == 0)
                return Result<Coordinates>.Failure(new CoordinatesNotFoundException("Address not found"));

            var location = geocodeResponse.Results[0].Geometry.Location;
            return Result<Coordinates>.Success(new Coordinates { Latitude = location.Lat, Longitude = location.Lng });
        }
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

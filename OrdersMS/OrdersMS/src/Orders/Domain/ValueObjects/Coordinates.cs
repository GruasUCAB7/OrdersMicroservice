using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Orders.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.ValueObjects
{
    public class Coordinates : IValueObject<Coordinates>
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public Coordinates(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
            {
                throw new InvalidCoordinatesException("Latitude must be between -90 and 90 degrees.");
            }

            if (longitude < -180 || longitude > 180)
            {
                throw new InvalidCoordinatesException("Longitude must be between -180 and 180 degrees.");
            }

            Latitude = latitude;
            Longitude = longitude;
        }

        public double GetLatitude()
        {
            return Latitude;
        }

        public double GetLongitude()
        {
            return Longitude;
        }

        public bool Equals(Coordinates other)
        {
            return Latitude == other.Latitude && Longitude == other.Longitude;
        }
    }
}

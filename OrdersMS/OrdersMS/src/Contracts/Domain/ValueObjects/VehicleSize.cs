using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class VehicleSize : IValueObject<VehicleSize>
    {
        public static readonly string Ligero = "Ligero";
        public static readonly string Mediano = "Mediano";
        public static readonly string Pesado = "Pesado";

        public string Size { get; }

        public VehicleSize(string size)
        {
            if (size != Ligero && size != Mediano && size != Pesado)
            {
                throw new InvalidPolicyTypeException($"Invalid policy type: {size}. Allowed values are: {Ligero}, {Mediano}, {Pesado}.");
            }
            Size = size;
        }

        public string GetValue()
        {
            return Size;
        }

        public bool Equals(VehicleSize other)
        {
            return Size == other.Size;
        }
    }
}

using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class PolicyType : IValueObject<PolicyType>
    {
        public static readonly string Basica = "Basica";
        public static readonly string Premium = "Premium";
        public static readonly string Diamante = "Diamante";

        public string Type { get; }

        public PolicyType(string type)
        {
            if (type != Basica && type != Premium && type != Diamante)
            {
                throw new InvalidPolicyTypeException($"Invalid policy type: {type}. Allowed values are: {Basica}, {Premium}, {Diamante}.");
            }
            Type = type;
        }

        public string GetValue()
        {
            return Type;
        }

        public bool Equals(PolicyType other)
        {
            return Type == other.Type;
        }
    }
}

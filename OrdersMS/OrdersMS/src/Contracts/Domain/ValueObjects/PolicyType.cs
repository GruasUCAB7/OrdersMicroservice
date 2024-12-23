using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class PolicyType : IValueObject<PolicyType>
    {
        public static readonly string Basica = "Basica";
        public static readonly string Premiun = "Premiun";
        public static readonly string Diamante = "Diamante";

        public string Type { get; }

        public PolicyType(string type)
        {
            if (type != Basica && type != Premiun && type != Diamante)
            {
                throw new InvalidPolicyTypeException($"Invalid policy type: {type}. Allowed values are: {Basica}, {Premiun}, {Diamante}.");
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

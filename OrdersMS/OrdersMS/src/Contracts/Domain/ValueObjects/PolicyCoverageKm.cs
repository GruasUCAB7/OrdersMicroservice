using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class PolicyCoverageKm : IValueObject<PolicyCoverageKm>
    {
        private decimal CoverageKm { get; }

        public PolicyCoverageKm(decimal coverageKm)
        {
            if (coverageKm < 1 || coverageKm > 1000000)
            {
                throw new InvalidPolicyCoverageKmException();
            }
            CoverageKm = coverageKm;
        }

        public decimal GetValue()
        {
            return CoverageKm;
        }

        public bool Equals(PolicyCoverageKm other)
        {
            return CoverageKm == other.CoverageKm;
        }
    }
}

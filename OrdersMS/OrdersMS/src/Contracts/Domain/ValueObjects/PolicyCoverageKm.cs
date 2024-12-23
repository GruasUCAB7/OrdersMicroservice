using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class PolicyCoverageKm : IValueObject<PolicyCoverageKm>
    {
        private int CoverageKm { get; }

        public PolicyCoverageKm(int coverageKm)
        {
            if (coverageKm < 1 || coverageKm > 1000000)
            {
                throw new InvalidPolicyCoverageKmException();
            }
            CoverageKm = coverageKm;
        }

        public int GetValue()
        {
            return CoverageKm;
        }

        public bool Equals(PolicyCoverageKm other)
        {
            return CoverageKm == other.CoverageKm;
        }
    }
}

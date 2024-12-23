using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class PolicyIncidentCoverageAmount : IValueObject<PolicyIncidentCoverageAmount>
    {
        private double CoverageAmount { get; }

        public PolicyIncidentCoverageAmount(double coverageAmount)
        {
            if (coverageAmount < 0 || coverageAmount > 1000000)
            {
                throw new InvalidPolicyIncidentCoverageAmountException();
            }
            CoverageAmount = coverageAmount;
        }

        public double GetValue()
        {
            return CoverageAmount;
        }

        public bool Equals(PolicyIncidentCoverageAmount other)
        {
            return CoverageAmount == other.CoverageAmount;
        }
    }
}

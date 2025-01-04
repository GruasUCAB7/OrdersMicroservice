using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class PolicyIncidentCoverageAmount : IValueObject<PolicyIncidentCoverageAmount>
    {
        private decimal CoverageAmount { get; }

        public PolicyIncidentCoverageAmount(decimal coverageAmount)
        {
            if (coverageAmount < 0 || coverageAmount > 1000000)
            {
                throw new InvalidPolicyIncidentCoverageAmountException();
            }
            CoverageAmount = coverageAmount;
        }

        public decimal GetValue()
        {
            return CoverageAmount;
        }

        public bool Equals(PolicyIncidentCoverageAmount other)
        {
            return CoverageAmount == other.CoverageAmount;
        }
    }
}

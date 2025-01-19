using OrdersMS.Core.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;

namespace OrdersMS.src.Contracts.Domain.Entities
{
    public class InsurancePolicy(PolicyId id, PolicyType type, PolicyCoverageKm coverageKm, PolicyIncidentCoverageAmount coverageAmount, PriceExtraKm pricePerExtraKm) : Entity<PolicyId>(id)
    {
        private PolicyId _id = id;
        private PolicyType _type = type;
        private PolicyCoverageKm _coverageKm = coverageKm;
        private PolicyIncidentCoverageAmount _coverageAmount = coverageAmount;
        private PriceExtraKm _pricePerExtraKm = pricePerExtraKm;
        private bool _isActive = true;

        public string GetId() => _id.GetValue();
        public string GetPolicyType() => _type.GetValue();
        public decimal GetPolicyCoverageKm() => _coverageKm.GetValue();
        public decimal GetPolicyIncidentCoverageAmount() => _coverageAmount.GetValue();
        public decimal GetPriceExtraKm() => _pricePerExtraKm.GetValue();
        public bool GetIsActive() => _isActive;
        public void SetIsActive(bool isActive) => _isActive = isActive;
        public void SetCoverageKm(PolicyCoverageKm coverageKm) => _coverageKm = coverageKm;
        public void SetIncidentCoverageAmount(PolicyIncidentCoverageAmount coverageAmount) => _coverageAmount = coverageAmount;
        public void SetPriceKmExtra(PriceExtraKm priceKm) => _pricePerExtraKm = priceKm;
    }
}

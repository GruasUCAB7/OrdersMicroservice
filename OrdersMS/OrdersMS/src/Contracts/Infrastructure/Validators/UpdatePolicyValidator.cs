using FluentValidation;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredPolicy.Types;

namespace OrdersMS.src.Contracts.Infrastructure.Validators
{
    public class UpdatePolicyValidator : AbstractValidator<UpdatePolicyCommand>
    {
        public UpdatePolicyValidator()
        {
            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("IsActive must be true or false.")
                .When(x => x.IsActive.HasValue);

            RuleFor(x => x.PolicyCoverageKm)
                .NotNull().WithMessage("Policy Coverage Km must not be null.")
                .GreaterThan(0).WithMessage("PolicyCoverageKm must be greater than 0.")
                .When(x => x.PolicyCoverageKm.HasValue);

            RuleFor(x => x.PolicyIncidentCoverageAmount)
                .NotNull().WithMessage("PolicyIncidentCoverageAmount must not be null.")
                .GreaterThan(0).WithMessage("PolicyIncidentCoverageAmount must be greater than 0.")
                .When(x => x.PolicyIncidentCoverageAmount.HasValue);

            RuleFor(x => x.PriceExtraKm)
                .NotNull().WithMessage("PriceExtraKm must not be null.")
                .GreaterThan(0).WithMessage("PriceExtraKm must be greater than 0.")
                .When(x => x.PriceExtraKm.HasValue);

        }
    }
}

using FluentValidation;
using OrdersMS.src.Contracts.Application.Commands.CreateInsurancePolicy.Types;

namespace OrdersMS.src.Contracts.Infrastructure.Validators
{
    public class CreatePolicyValidator : AbstractValidator<CreatePolicyCommand>
    {
        public CreatePolicyValidator()
        {
            RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.");

            RuleFor(x => x.CoverageKm)
            .NotEmpty().WithMessage("Coverage Km is required.");

            RuleFor(x => x.CoverageAmount)
            .NotEmpty().WithMessage("Coverage amount is required.");

            RuleFor(x => x.PriceExtraKm)
            .NotEmpty().WithMessage("Price per extra Km is required.");
        }
    }
}

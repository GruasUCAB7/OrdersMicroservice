using FluentValidation;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredPolicy.Types;

namespace OrdersMS.src.Contracts.Infrastructure.Validators
{
    public class UpdatePolicyValidator : AbstractValidator<UpdatePolicyCommand>
    {
        public UpdatePolicyValidator()
        {
            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("IsActive must be true or false.");

        }
    }
}

using FluentValidation;
using OrdersMS.src.Contracts.Application.Commands.CreateContract.Types;

namespace OrdersMS.src.Contracts.Infrastructure.Validators
{
    public class CreateContractValidator : AbstractValidator<CreateContractCommand>
    {
        public CreateContractValidator()
        {
            RuleFor(x => x.AssociatedPolicy)
            .NotEmpty().WithMessage("Policy ID is required.");

            RuleFor(x => x.InsuredVehicle)
            .NotEmpty().WithMessage("Vehicle ID is required.");
        }
    }
}

using FluentValidation;
using OrdersMS.src.Contracts.Application.Commands.UpdateContract.Types;

namespace OrdersMS.src.Contracts.Infrastructure.Validators
{
    public class UpdateContractValidator : AbstractValidator<UpdateContractCommand>
    {
        public UpdateContractValidator()
        {
            RuleFor(x => x.Status)
                .NotNull().WithMessage("Status must be true or false.");

        }
    }
}

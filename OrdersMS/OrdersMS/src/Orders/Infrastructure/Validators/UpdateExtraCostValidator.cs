using FluentValidation;
using OrdersMS.src.Orders.Application.Commands.UpdateExtraCost.Types;

namespace OrdersMS.src.Orders.Infrastructure.Validators
{
    public class UpdateExtraCostValidator : AbstractValidator<UpdateExtraCostCommand>
    {
        public UpdateExtraCostValidator()
        {
            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("IsActive must be true or false.");

        }
    }
}

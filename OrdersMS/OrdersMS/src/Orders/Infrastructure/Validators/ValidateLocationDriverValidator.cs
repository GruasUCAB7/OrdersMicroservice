using FluentValidation;
using OrdersMS.src.Orders.Application.Commands.ValidateLocationDriverToIncidecident.Types;

namespace OrdersMS.src.Orders.Infrastructure.Validators
{
    public class ValidateLocationDriverValidator : AbstractValidator<ValidateLocationCommand>
    {
        public ValidateLocationDriverValidator()
        {
            RuleFor(x => x.DriverLocationResponse)
                .NotNull().WithMessage("Driver response must be true or false.")
                .Must(response => response == true || response == false)
                .WithMessage("Driver location response must be true or false.");
        }
    }
}

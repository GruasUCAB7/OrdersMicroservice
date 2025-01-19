using FluentValidation;
using OrdersMS.src.Orders.Application.Commands.UpdateDriverAssigned.Types;

namespace OrdersMS.src.Orders.Infrastructure.Validators
{
    public class UpdateDriverAssignedValidator : AbstractValidator<UpdateDriverAssignedCommand>
    {
        public UpdateDriverAssignedValidator()
        {
            RuleFor(x => x.DriverAssigned)
                .NotEmpty().WithMessage("Driver ID is required.");
        }
    }
}

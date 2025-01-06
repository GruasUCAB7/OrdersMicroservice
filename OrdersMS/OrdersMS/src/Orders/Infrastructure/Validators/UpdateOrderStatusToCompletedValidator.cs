using FluentValidation;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToCompleted.Types;

namespace OrdersMS.src.Orders.Infrastructure.Validators
{
    public class UpdateOrderStatusToCompletedValidator : AbstractValidator<UpdateOrderStatusToCompletedCommand>
    {
        public UpdateOrderStatusToCompletedValidator()
        {
            RuleFor(x => x.OrderCompletedDriverResponse)
                .NotNull().WithMessage("Driver response of order completed must be true or false.")
                .Must(response => response == true || response == false);
        }
    }
}

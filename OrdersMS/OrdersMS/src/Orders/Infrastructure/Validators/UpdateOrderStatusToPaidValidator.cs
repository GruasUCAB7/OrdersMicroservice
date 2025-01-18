using FluentValidation;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToPaid.Types;

namespace OrdersMS.src.Orders.Infrastructure.Validators
{
    public class UpdateOrderStatusToPaidValidator : AbstractValidator<UpdateOrderStatusToPaidCommand>
    {
        public UpdateOrderStatusToPaidValidator()
        {
            RuleFor(x => x.OrderPaidResponse)
                .NotNull().WithMessage("Response of order paid must be true or false.")
                .Must(response => response == true || response == false);
        }
    }
}

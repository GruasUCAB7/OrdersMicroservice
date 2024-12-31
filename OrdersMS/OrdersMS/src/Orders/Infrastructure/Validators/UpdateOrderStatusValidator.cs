using FluentValidation;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatus.Types;

namespace OrdersMS.src.Orders.Infrastructure.Validators
{
    public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
    {
        public UpdateOrderStatusValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Order status is required.");
        }
    }
}

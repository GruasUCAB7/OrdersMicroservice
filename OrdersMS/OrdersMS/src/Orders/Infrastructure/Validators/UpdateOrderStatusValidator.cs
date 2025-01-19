using FluentValidation;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatus.Types;

namespace OrdersMS.src.Orders.Infrastructure.Validators
{
    public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
    {
        public UpdateOrderStatusValidator()
        {
            RuleFor(x => x.OrderAcceptedDriverResponse)
                .NotNull().WithMessage("Driver response of order accepted must be true or false.")
                .Must(response => response == true || response == false)
                .When(x => x.OrderAcceptedDriverResponse.HasValue);

            RuleFor(x => x.OrderInProcessDriverResponse)
                .NotNull().WithMessage("Driver response of order in process must be true or false.")
                .Must(response => response == true || response == false)
                .When(x => x.OrderInProcessDriverResponse.HasValue);

            RuleFor(x => x.OrderCanceledDriverResponse)
                .NotNull().WithMessage("Driver response of order canceled must be true or false.")
                .Must(response => response == true || response == false)
                .When(x => x.OrderCanceledDriverResponse.HasValue);
        }
    }
}

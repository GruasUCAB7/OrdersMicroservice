using FluentValidation;
using OrdersMS.src.Orders.Application.Commands.CreateOrder.Types;

namespace OrdersMS.src.Orders.Infrastructure.Validators
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderValidator()
        {
            RuleFor(x => x.ContractId)
                .NotEmpty().WithMessage("Contract ID is required.");

            RuleFor(x => x.DriverAssigned)
            .NotNull().WithMessage("Driver assigned is required.")
            .When(x => x.DriverAssigned != null);

            RuleFor(x => x.IncidentAddress)
                .NotEmpty().WithMessage("Incident Address is required.");

            RuleFor(x => x.DestinationAddress)
                .NotEmpty().WithMessage("Destination Address is required.");
        }
    }
}

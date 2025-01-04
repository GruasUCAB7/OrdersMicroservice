using FluentValidation;
using OrdersMS.src.Orders.Application.Commands.UpdateTotalAmountOrder.Types;

namespace OrdersMS.src.Orders.Infrastructure.Validators
{
    public class UpdateTotalAmountOrderValidator : AbstractValidator<UpdateTotalAmountOrderCommand>
    {
        public UpdateTotalAmountOrderValidator()
        {
            RuleFor(x => x.TotalKmTraveled)
                .NotEmpty().WithMessage("Total Km traveled are required.");
        }
    }
}

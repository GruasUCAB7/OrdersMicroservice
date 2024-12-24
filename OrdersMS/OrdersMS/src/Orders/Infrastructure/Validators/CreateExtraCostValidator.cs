using FluentValidation;
using OrdersMS.src.Orders.Application.Commands.CreateExtraCost.Types;

namespace OrdersMS.src.Orders.Infrastructure.Validators
{
    public class CreateExtraCostValidator : AbstractValidator<CreateExtraCostCommand>
    {
        public CreateExtraCostValidator()
        {
            RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
        }
    }
}

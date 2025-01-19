using FluentValidation;
using OrdersMS.src.Orders.Application.Commands.CreateExtraCost.Types;

namespace OrdersMS.src.Orders.Infrastructure.Validators
{
    public class CreateExtraCostValidator : AbstractValidator<CreateExtraCostCommand>
    {
        public CreateExtraCostValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("OrderId is required.");

            RuleForEach(x => x.ExtraCosts).ChildRules(extraCost =>
            {
                extraCost.RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Name is required.");

                extraCost.RuleFor(x => x.Price)
                    .GreaterThan(0).WithMessage("Price must be greater than zero.");
            });
        }
    }
}

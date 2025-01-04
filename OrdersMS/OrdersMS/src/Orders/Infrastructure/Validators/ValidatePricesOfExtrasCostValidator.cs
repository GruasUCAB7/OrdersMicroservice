using FluentValidation;
using OrdersMS.src.Orders.Application.Commands.ValidatePricesOfExtrasCost.Types;

namespace OrdersMS.src.Orders.Infrastructure.Validators
{
    public class ValidatePricesOfExtrasCostValidator : AbstractValidator<ValidatePricesOfExtrasCostCommand>
    {
        public ValidatePricesOfExtrasCostValidator()
        {
            RuleFor(x => x.OperatorRespose)
                .NotNull().WithMessage("Response of operator must be true or false.")
                .Must(response => response == true || response == false);

            RuleForEach(x => x.ExtrasCostApplied)
                .ChildRules(extraCost =>
                {
                    extraCost.RuleFor(x => x.Name)
                        .NotEmpty().WithMessage("Name is required.");

                    extraCost.RuleFor(x => x.Price)
                        .GreaterThan(0).WithMessage("Price must be greater than zero.");
                });
        }
    }
}

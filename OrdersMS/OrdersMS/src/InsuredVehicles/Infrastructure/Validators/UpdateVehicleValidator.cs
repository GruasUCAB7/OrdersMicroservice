using FluentValidation;
using OrdersMS.src.InsuredVehicles.Application.Commands.UpdateInsuredVehicle.Types;

namespace OrdersMS.src.InsuredVehicles.Infrastructure.Validators
{
    public class UpdateVehicleValidator : AbstractValidator<UpdateVehicleCommand>
    {
        public UpdateVehicleValidator()
        {
            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("IsActive must be true or false.");

        }
    }
}

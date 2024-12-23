using FluentValidation;
using OrdersMS.src.Contracts.Application.Commands.CreateInsuredVehicle.Types;

namespace OrdersMS.src.Contracts.Infrastructure.Validators
{
    public class CreateVehicleValidator : AbstractValidator<CreateVehicleCommand>
    {
        public CreateVehicleValidator()
        {
            RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .MinimumLength(2).WithMessage("The brand must not be less than 2 characters.")
            .MaximumLength(20).WithMessage("Brand must be less than 20 characters.");

            RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required.")
            .MinimumLength(2).WithMessage("The model must not be less than 2 characters.")
            .MaximumLength(20).WithMessage("Model must be less than 20 characters.");

            RuleFor(x => x.Plate)
            .NotEmpty().WithMessage("Plate is required.");

            RuleFor(x => x.VehicleSize)
            .NotEmpty().WithMessage("Crane type is required.");

            RuleFor(x => x.Year)
            .NotEmpty().WithMessage("Year of de crane is required.");

            RuleFor(x => x.ClientDNI)
            .NotEmpty().WithMessage("Client DNI is required.")
            .MinimumLength(7).WithMessage("The client DNI must not be less than 7 characters.")
            .MaximumLength(8).WithMessage("Client DNI must be less than 8 characters.");

            RuleFor(x => x.ClientName)
                .NotEmpty().WithMessage("Name is required.")
                .MinimumLength(3).WithMessage("The name must not be less than 3 characters.")
                .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

            RuleFor(x => x.ClientEmail)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is not valid.");
        }
    }
}

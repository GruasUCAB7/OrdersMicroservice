namespace OrdersMS.src.Contracts.Application.Commands.CreateContract.Types
{
    public record CreateContractCommand(
        string AssociatedPolicy,
        string InsuredVehicle,
        string Status
    );
}

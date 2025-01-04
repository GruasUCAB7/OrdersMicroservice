using OrdersMS.src.Orders.Application.Commands.AddExtraCost.Types;

namespace OrdersMS.src.Orders.Application.Commands.ValidatePricesOfExtrasCost.Types
{
    public record ValidatePricesOfExtrasCostCommand
    (
        bool OperatorRespose,
        List<ExtraCostDto> ExtrasCostApplied
    );
}

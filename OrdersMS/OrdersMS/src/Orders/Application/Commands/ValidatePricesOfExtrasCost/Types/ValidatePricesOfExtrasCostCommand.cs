using OrdersMS.src.Orders.Application.Queries.GetExtraCostsByOrderId.Types;

namespace OrdersMS.src.Orders.Application.Commands.ValidatePricesOfExtrasCost.Types
{
    public record ValidatePricesOfExtrasCostCommand
    (
        bool OperatorRespose,
        List<ExtraCostDto>? ExtrasCostApplied
    );
}

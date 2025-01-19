using OrdersMS.src.Orders.Application.Queries.GetExtraCostsByOrderId.Types;

namespace OrdersMS.src.Orders.Application.Commands.AddExtraCost.Types
{
    public record AddExtraCostCommand(
        List<ExtraCostDto> ExtraCosts
    );
}
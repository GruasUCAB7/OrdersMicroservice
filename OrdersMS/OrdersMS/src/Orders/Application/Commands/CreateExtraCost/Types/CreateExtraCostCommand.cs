namespace OrdersMS.src.Orders.Application.Commands.CreateExtraCost.Types
{
    public record CreateExtraCostCommand(
        string OrderId,
        List<ExtraCostDto2> ExtraCosts
    );

    public record ExtraCostDto2(
        string Name,
        decimal Price
    );
}

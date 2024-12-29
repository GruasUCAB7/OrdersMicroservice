namespace OrdersMS.src.Orders.Application.Commands.AddExtraCost.Types
{
    public record AddExtraCostCommand(
        string OrderId,
        List<ExtraCostDto> ExtraCosts
    );

    public record ExtraCostDto(
        string Name,
        decimal Price
    );
}
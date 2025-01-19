namespace OrdersMS.src.Orders.Application.Queries.GetExtraCostsByOrderId.Types
{
    public record GetExtraCostResponse(
        List<ExtraCostDto> ExtraCosts
    );

    public record ExtraCostDto(
        string Id,
        string Name,
        decimal Price
    );
}
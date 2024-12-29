namespace OrdersMS.src.Orders.Application.Queries.GetAllOrders.Types
{
    public record GetAllOrdersQuery
    (
        int PerPage,
        int Page,
        string? Status
    );
}

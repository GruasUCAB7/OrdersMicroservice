namespace OrdersMS.src.Orders.Application.Queries.GetAllExtraCosts.Types
{
    public record GetAllExtraCostsQuery
    (
        int PerPage,
        int Page,
        string? IsActive
    );
}

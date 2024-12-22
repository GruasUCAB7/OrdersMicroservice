namespace OrdersMS.src.Contracts.Application.Queries.GetAllVehicles.Types
{
    public record GetAllVehiclesQuery
    (
        int PerPage,
        int Page,
        string? IsActive
    );
}

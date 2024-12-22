namespace OrdersMS.src.InsuredVehicles.Application.Queries.GetAll.Types
{
    public record GetAllVehiclesQuery
    (
        int PerPage,
        int Page,
        string? IsActive
    );
}

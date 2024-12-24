namespace OrdersMS.src.Contracts.Application.Queries.GetAllContracts.Types
{
    public record GetAllContractsQuery
    (
        int PerPage,
        int Page,
        string? Status
    );
}

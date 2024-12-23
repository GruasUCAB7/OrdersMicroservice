namespace OrdersMS.src.Contracts.Application.Queries.GetAllPolicies.Types
{
    public record GetAllPoliciesQuery
    (
        int PerPage,
        int Page,
        string? IsActive
    );
}

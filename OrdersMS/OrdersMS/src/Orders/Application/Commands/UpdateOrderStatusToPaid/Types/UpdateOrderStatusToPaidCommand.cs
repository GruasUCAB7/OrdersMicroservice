namespace OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToPaid.Types
{
    public record UpdateOrderStatusToPaidCommand
    (
        string OperatorId,
        bool OrderPaidResponse
    );
}

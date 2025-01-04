namespace OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToCompleted.Types
{
    public record UpdateOrderStatusToCompletedCommand
    (
        string DriverAssigned,
        bool OrderCompletedDriverResponse
    );
}

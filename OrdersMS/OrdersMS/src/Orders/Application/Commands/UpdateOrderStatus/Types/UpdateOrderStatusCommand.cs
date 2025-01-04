namespace OrdersMS.src.Orders.Application.Commands.UpdateOrderStatus.Types
{
    public record UpdateOrderStatusCommand
    (
        string DriverId,
        bool? OrderAcceptedDriverResponse,
        bool? OrderInProcessDriverResponse,
        bool? OrderCanceledDriverResponse
    );
}

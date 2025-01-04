namespace OrdersMS.src.Orders.Application.Commands.UpdateOrderStatus.Types
{
    public record UpdateOrderStatusCommand
    (
        bool? OrderAcceptedDriverResponse,
        bool? OrderInProcessDriverResponse,
        bool? OrderCanceledDriverResponse
    );
}

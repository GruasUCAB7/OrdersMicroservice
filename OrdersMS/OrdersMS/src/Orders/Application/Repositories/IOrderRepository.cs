﻿using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders.Types;
using OrdersMS.src.Orders.Application.Queries.GetAllOrdersByDriverAssigned.Types;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Application.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAll(GetAllOrdersQuery data);
        Task<Optional<Order>> GetById(string id);
        Task<Result<Order>> Save(Order order);
        Task<Result<Order>> Update(Order order);
        Task<Result<Order>> UpdateExtraCosts(OrderId orderId, List<ExtraCost> extraCosts);
        Task<List<Order>> ValidateUpdateTimeForStatusPorAceptar();
        Task<List<Order>> GetAllOrdersByDriverAssigned(GetAllOrdersByDriverAssignedQuery data, string driverId);
        Task<string> GetDriverDeviceToken(string driverId);
    }
}

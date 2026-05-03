using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Application.DTOs.Orders;

public record AdminOrderResponse(
    int Id,
    string UserName,
    DateTime OrderDate,
    decimal TotalAmount,
    OrderStatus Status,
    string Address
);

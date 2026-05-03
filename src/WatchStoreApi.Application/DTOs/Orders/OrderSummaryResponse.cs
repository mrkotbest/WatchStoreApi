using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Application.DTOs.Orders;

public record OrderSummaryResponse(
    int Id,
    decimal TotalAmount,
    DateTime OrderDate,
    OrderStatus Status
);

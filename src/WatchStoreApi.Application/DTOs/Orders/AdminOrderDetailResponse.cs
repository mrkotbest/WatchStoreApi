namespace WatchStoreApi.Application.DTOs.Orders;

public record AdminOrderDetailResponse(
    int Id,
    int Qty,
    decimal TotalAmount,
    int ProductId,
    string ProductName,
    string? ProductImageUrl,
    decimal ProductPrice
);

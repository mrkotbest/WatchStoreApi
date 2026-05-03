namespace WatchStoreApi.Application.DTOs.Orders;

public record OrderDetailResponse(
    int Id,
    int Qty,
    decimal UnitPrice,
    decimal TotalAmount,
    int ProductId,
    string ProductName,
    string? ProductImageUrl,
    decimal ProductPrice
);

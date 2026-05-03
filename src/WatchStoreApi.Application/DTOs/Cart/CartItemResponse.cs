namespace WatchStoreApi.Application.DTOs.Cart;

public record CartItemResponse(
    int Id,
    int Qty,
    decimal UnitPrice,
    decimal TotalAmount,
    int ProductId,
    string ProductName,
    string? ImageUrl
);

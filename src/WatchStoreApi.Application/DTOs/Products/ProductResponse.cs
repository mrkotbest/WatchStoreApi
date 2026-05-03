using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Application.DTOs.Products;

public record ProductResponse(
    int Id,
    string Name,
    string Description,
    string? ImageUrl,
    string Material,
    Gender Gender,
    decimal Price,
    int CategoryId
);

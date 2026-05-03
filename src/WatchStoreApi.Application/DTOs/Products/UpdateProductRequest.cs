using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Application.DTOs.Products;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
}

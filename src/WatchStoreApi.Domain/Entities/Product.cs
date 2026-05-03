using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Material { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public decimal Price { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<ShoppingCartItem> ShoppingCartItems { get; set; } = [];
    public ICollection<OrderDetail> OrderDetails { get; set; } = [];
}

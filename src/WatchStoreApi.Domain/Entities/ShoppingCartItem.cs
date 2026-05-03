namespace WatchStoreApi.Domain.Entities;

public class ShoppingCartItem
{
    public int Id { get; set; }
    public decimal UnitPrice { get; set; }
    public int Qty { get; set; }
    public decimal TotalAmount { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
}

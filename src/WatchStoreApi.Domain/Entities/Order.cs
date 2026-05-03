using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = [];
}

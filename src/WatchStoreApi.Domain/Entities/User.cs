using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; }

    public ICollection<ShoppingCartItem> ShoppingCartItems { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

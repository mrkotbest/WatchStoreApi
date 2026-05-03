using Moq;
using WatchStoreApi.Application.DTOs.Cart;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Application.Services;
using WatchStoreApi.Domain.Entities;
using WatchStoreApi.Infrastructure.Persistence;

namespace WatchStoreApi.Tests.Unit.Services;

public class CartServiceTests : IDisposable
{
    private const int TestUserId = 100;

    private readonly AppDbContext _db;
    private readonly CartService _sut;
    private readonly Mock<ICurrentUserService> _currentUserMock;

    public CartServiceTests()
    {
        _db = DbContextFactory.Create();
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns(TestUserId);

        _db.Users.Add(new User
        {
            Id = TestUserId, Name = "Test", Email = "cart@test.com",
            PasswordHash = "hash", CreatedAt = DateTime.UtcNow
        });
        _db.SaveChanges();

        _sut = new CartService(_db, _currentUserMock.Object);
    }

    [Fact]
    public async Task AddToCart_ValidProduct_Succeeds()
    {
        var result = await _sut.AddToCartAsync(new AddToCartRequest(1, 2));

        Assert.True(result.IsSuccess);
        var item = _db.ShoppingCartItems.First(s => s.UserId == TestUserId && s.ProductId == 1);
        Assert.Equal(2, item.Qty);
    }

    [Fact]
    public async Task AddToCart_SetsCorrectPrice()
    {
        await _sut.AddToCartAsync(new AddToCartRequest(1, 1));

        var item = _db.ShoppingCartItems.First(s => s.UserId == TestUserId);
        var product = _db.Products.Find(1)!;
        Assert.Equal(product.Price, item.UnitPrice);
        Assert.Equal(product.Price, item.TotalAmount);
    }

    [Fact]
    public async Task AddToCart_ExistingItem_IncreasesQty()
    {
        await _sut.AddToCartAsync(new AddToCartRequest(1, 2));
        await _sut.AddToCartAsync(new AddToCartRequest(1, 3));

        var item = _db.ShoppingCartItems.First(s => s.UserId == TestUserId && s.ProductId == 1);
        Assert.Equal(5, item.Qty);
    }

    [Fact]
    public async Task AddToCart_NonExistentProduct_ReturnsNotFound()
    {
        var result = await _sut.AddToCartAsync(new AddToCartRequest(9999, 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task AddToCart_UsesCurrentUserNotRequestBody()
    {
        await _sut.AddToCartAsync(new AddToCartRequest(1, 1));

        var item = _db.ShoppingCartItems.First();
        Assert.Equal(TestUserId, item.UserId);
        _currentUserMock.Verify(x => x.UserId, Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetCartItems_ReturnsOnlyCurrentUserItems()
    {
        _db.ShoppingCartItems.Add(new ShoppingCartItem
            { UserId = TestUserId, ProductId = 1, Qty = 1, UnitPrice = 100, TotalAmount = 100 });
        _db.ShoppingCartItems.Add(new ShoppingCartItem
            { UserId = 999, ProductId = 2, Qty = 1, UnitPrice = 200, TotalAmount = 200 });
        await _db.SaveChangesAsync();

        var items = await _sut.GetCartItemsAsync();

        Assert.Single(items);
        Assert.Equal(1, items[0].ProductId);
    }

    [Fact]
    public async Task UpdateCartItem_Increase_Works()
    {
        await _sut.AddToCartAsync(new AddToCartRequest(1, 2));

        var result = await _sut.UpdateCartItemAsync(1, new UpdateCartItemRequest("increase"));

        Assert.True(result.IsSuccess);
        var item = _db.ShoppingCartItems.First(s => s.UserId == TestUserId && s.ProductId == 1);
        Assert.Equal(3, item.Qty);
    }

    [Fact]
    public async Task UpdateCartItem_Decrease_Works()
    {
        await _sut.AddToCartAsync(new AddToCartRequest(1, 3));

        var result = await _sut.UpdateCartItemAsync(1, new UpdateCartItemRequest("decrease"));

        Assert.True(result.IsSuccess);
        var item = _db.ShoppingCartItems.First(s => s.UserId == TestUserId && s.ProductId == 1);
        Assert.Equal(2, item.Qty);
    }

    [Fact]
    public async Task UpdateCartItem_DecreaseToZero_RemovesItem()
    {
        await _sut.AddToCartAsync(new AddToCartRequest(1, 1));

        await _sut.UpdateCartItemAsync(1, new UpdateCartItemRequest("decrease"));

        Assert.Empty(_db.ShoppingCartItems.Where(s => s.UserId == TestUserId && s.ProductId == 1));
    }

    [Fact]
    public async Task UpdateCartItem_InvalidAction_ReturnsFailure()
    {
        await _sut.AddToCartAsync(new AddToCartRequest(1, 1));

        var result = await _sut.UpdateCartItemAsync(1, new UpdateCartItemRequest("invalid"));

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RemoveFromCart_ExistingItem_Succeeds()
    {
        await _sut.AddToCartAsync(new AddToCartRequest(1, 2));

        var result = await _sut.RemoveFromCartAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Empty(_db.ShoppingCartItems.Where(s => s.UserId == TestUserId));
    }

    [Fact]
    public async Task RemoveFromCart_NonExistent_ReturnsNotFound()
    {
        var result = await _sut.RemoveFromCartAsync(9999);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }
}

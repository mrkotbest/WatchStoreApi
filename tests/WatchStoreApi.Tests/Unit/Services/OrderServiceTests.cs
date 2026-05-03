using Moq;
using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Orders;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Application.Services;
using WatchStoreApi.Domain.Entities;
using WatchStoreApi.Domain.Enums;
using WatchStoreApi.Infrastructure.Persistence;

namespace WatchStoreApi.Tests.Unit.Services;

public class OrderServiceTests : IDisposable
{
    private const int TestUserId = 200;

    private readonly AppDbContext _db;
    private readonly OrderService _sut;
    private readonly Mock<ICurrentUserService> _currentUserMock;

    public OrderServiceTests()
    {
        _db = DbContextFactory.Create();
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns(TestUserId);

        _db.Users.Add(new User
        {
            Id = TestUserId, Name = "OrderUser", Email = "order@test.com",
            PasswordHash = "hash", CreatedAt = DateTime.UtcNow
        });
        _db.SaveChanges();

        _sut = new OrderService(_db, _currentUserMock.Object);
    }

    private void AddCartItems(int count = 2)
    {
        for (var i = 1; i <= count; i++)
        {
            _db.ShoppingCartItems.Add(new ShoppingCartItem
            {
                UserId = TestUserId,
                ProductId = i,
                Qty = i,
                UnitPrice = 100m * i,
                TotalAmount = 100m * i * i
            });
        }
        _db.SaveChanges();
    }

    [Fact]
    public async Task CreateOrder_WithCartItems_Succeeds()
    {
        AddCartItems();

        var result = await _sut.CreateOrderAsync(new CreateOrderRequest("123 Test St"));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value > 0);
    }

    [Fact]
    public async Task CreateOrder_SetsCorrectFields()
    {
        AddCartItems();

        var result = await _sut.CreateOrderAsync(new CreateOrderRequest("456 Main St"));

        var order = _db.Orders.Find(result.Value);
        Assert.NotNull(order);
        Assert.Equal(TestUserId, order.UserId);
        Assert.Equal("456 Main St", order.Address);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.True(order.TotalAmount > 0);
        Assert.True(order.OrderDate <= DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateOrder_UsesCurrentUserIdNotRequestBody()
    {
        AddCartItems();

        await _sut.CreateOrderAsync(new CreateOrderRequest("Addr"));

        var order = _db.Orders.First();
        Assert.Equal(TestUserId, order.UserId);
        _currentUserMock.Verify(x => x.UserId, Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateOrder_ClearsCart()
    {
        AddCartItems();

        await _sut.CreateOrderAsync(new CreateOrderRequest("Addr"));

        var remaining = _db.ShoppingCartItems.Where(s => s.UserId == TestUserId).ToList();
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task CreateOrder_CreatesOrderDetails()
    {
        AddCartItems(3);

        var result = await _sut.CreateOrderAsync(new CreateOrderRequest("Addr"));

        var details = _db.OrderDetails.Where(od => od.OrderId == result.Value).ToList();
        Assert.Equal(3, details.Count);
    }

    [Fact]
    public async Task CreateOrder_EmptyCart_ReturnsFailure()
    {
        var result = await _sut.CreateOrderAsync(new CreateOrderRequest("Addr"));

        Assert.False(result.IsSuccess);
        Assert.Contains("empty", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetMyOrders_ReturnsOnlyCurrentUserOrders()
    {
        AddCartItems();
        await _sut.CreateOrderAsync(new CreateOrderRequest("My Addr"));

        _db.Orders.Add(new Order
        {
            UserId = 999, Address = "Other", TotalAmount = 100,
            OrderDate = DateTime.UtcNow, Status = OrderStatus.Pending
        });
        await _db.SaveChangesAsync();

        var result = await _sut.GetMyOrdersAsync(new PagedRequest());

        Assert.All(result.Items, o => Assert.Equal(TestUserId,
            _db.Orders.Find(o.Id)!.UserId));
    }

    [Fact]
    public async Task GetMyOrderDetails_OwnOrder_Succeeds()
    {
        AddCartItems();
        var orderResult = await _sut.CreateOrderAsync(new CreateOrderRequest("Addr"));

        var result = await _sut.GetMyOrderDetailsAsync(orderResult.Value);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Count > 0);
    }

    [Fact]
    public async Task GetMyOrderDetails_OtherUsersOrder_ReturnsNotFound()
    {
        _db.Orders.Add(new Order
        {
            UserId = 999, Address = "Other", TotalAmount = 100,
            OrderDate = DateTime.UtcNow, Status = OrderStatus.Pending
        });
        await _db.SaveChangesAsync();
        var otherOrderId = _db.Orders.First(o => o.UserId == 999).Id;

        var result = await _sut.GetMyOrderDetailsAsync(otherOrderId);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task GetMyOrderDetails_NonExistentOrder_ReturnsNotFound()
    {
        var result = await _sut.GetMyOrderDetailsAsync(9999);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }
}

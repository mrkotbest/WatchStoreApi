using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Orders;
using WatchStoreApi.Application.Services;
using WatchStoreApi.Domain.Entities;
using WatchStoreApi.Domain.Enums;
using WatchStoreApi.Infrastructure.Persistence;

namespace WatchStoreApi.Tests.Unit.Services;

public class AdminServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AdminService _sut;

    public AdminServiceTests()
    {
        _db = DbContextFactory.Create();
        _sut = new AdminService(_db);
        SeedOrders();
    }

    private void SeedOrders()
    {
        _db.Users.Add(new User
        {
            Id = 300, Name = "Admin Test", Email = "admin@test.com",
            PasswordHash = "h", CreatedAt = DateTime.UtcNow
        });

        _db.Orders.AddRange(
            new Order { UserId = 300, Address = "A1", TotalAmount = 100, OrderDate = DateTime.UtcNow, Status = OrderStatus.Pending },
            new Order { UserId = 300, Address = "A2", TotalAmount = 200, OrderDate = DateTime.UtcNow, Status = OrderStatus.Delivered },
            new Order { UserId = 300, Address = "A3", TotalAmount = 300, OrderDate = DateTime.UtcNow, Status = OrderStatus.Delivered },
            new Order { UserId = 300, Address = "A4", TotalAmount = 150, OrderDate = DateTime.UtcNow, Status = OrderStatus.Cancelled }
        );
        _db.SaveChanges();
    }

    [Fact]
    public async Task GetDashboard_ReturnsCorrectCounts()
    {
        var result = await _sut.GetDashboardAsync();

        Assert.Equal(4, result.TotalOrders);
        Assert.Equal(1, result.PendingOrders);
        Assert.Equal(500, result.TotalRevenue);
        Assert.True(result.TotalProducts >= 20);
        Assert.True(result.TotalCategories >= 4);
    }

    [Fact]
    public async Task GetAllOrders_ReturnsAll()
    {
        var result = await _sut.GetAllOrdersAsync(1, 10, null, null, null, null);

        Assert.Equal(4, result.TotalCount);
    }

    [Fact]
    public async Task GetAllOrders_FilterByStatus_Works()
    {
        var result = await _sut.GetAllOrdersAsync(1, 10, "Pending", null, null, null);

        Assert.All(result.Items, o => Assert.Equal(OrderStatus.Pending, o.Status));
    }

    [Fact]
    public async Task GetPendingOrders_ReturnsOnlyPending()
    {
        var result = await _sut.GetPendingOrdersAsync(new PagedRequest());

        Assert.All(result.Items, o => Assert.Equal(OrderStatus.Pending, o.Status));
    }

    [Fact]
    public async Task UpdateOrderStatus_ValidTransition_Succeeds()
    {
        var pendingOrder = _db.Orders.First(o => o.Status == OrderStatus.Pending);

        var result = await _sut.UpdateOrderStatusAsync(
            pendingOrder.Id, new UpdateOrderStatusRequest(OrderStatus.Confirmed));

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Confirmed, _db.Orders.Find(pendingOrder.Id)!.Status);
    }

    [Fact]
    public async Task UpdateOrderStatus_InvalidTransition_ReturnsFailure()
    {
        var pendingOrder = _db.Orders.First(o => o.Status == OrderStatus.Pending);

        var result = await _sut.UpdateOrderStatusAsync(
            pendingOrder.Id, new UpdateOrderStatusRequest(OrderStatus.Delivered));

        Assert.False(result.IsSuccess);
        Assert.Contains("Cannot transition", result.Error);
    }

    [Fact]
    public async Task UpdateOrderStatus_NonExistentOrder_ReturnsNotFound()
    {
        var result = await _sut.UpdateOrderStatusAsync(
            9999, new UpdateOrderStatusRequest(OrderStatus.Confirmed));

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task GetRevenue_Monthly_Returns7Periods()
    {
        var result = await _sut.GetRevenueAsync("monthly");

        Assert.Equal(7, result.Count);
    }

    [Fact]
    public async Task GetRevenue_InvalidRange_ReturnsEmpty()
    {
        var result = await _sut.GetRevenueAsync("invalid");

        Assert.Empty(result);
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }
}

using Microsoft.EntityFrameworkCore;
using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Orders;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Application.Interfaces.Persistence;
using WatchStoreApi.Domain.Entities;
using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Application.Services;

public class OrderService(IAppDbContext dbContext, ICurrentUserService currentUser) : IOrderService
{
    public async Task<Result<int>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var cartItems = await dbContext.ShoppingCartItems
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
            return Result<int>.Failure("Shopping cart is empty.");

        await using var transaction = await dbContext.BeginTransactionAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var order = new Order
        {
            UserId = userId,
            Address = request.Address,
            OrderDate = now,
            Status = OrderStatus.Pending,
            TotalAmount = cartItems.Sum(c => c.TotalAmount),
            CreatedAt = now
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var cartItem in cartItems)
        {
            dbContext.OrderDetails.Add(new OrderDetail
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                UnitPrice = cartItem.UnitPrice,
                Qty = cartItem.Qty,
                TotalAmount = cartItem.TotalAmount
            });
        }

        dbContext.ShoppingCartItems.RemoveRange(cartItems);
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Result<int>.Created(order.Id);
    }

    public Task<PagedResponse<OrderSummaryResponse>> GetMyOrdersAsync(PagedRequest paging, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        return dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderSummaryResponse(o.Id, o.TotalAmount, o.OrderDate, o.Status))
            .ToPagedResponseAsync(paging.PageNumber, paging.PageSize, cancellationToken);
    }

    public async Task<Result<IReadOnlyList<OrderDetailResponse>>> GetMyOrderDetailsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var orderExists = await dbContext.Orders
            .AnyAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken);

        if (!orderExists)
            return Result<IReadOnlyList<OrderDetailResponse>>.NotFound("Order not found.");

        var details = await dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.OrderId == orderId)
            .Select(od => new OrderDetailResponse(
                od.Id, od.Qty, od.UnitPrice, od.TotalAmount,
                od.ProductId, od.Product!.Name, od.Product.ImageUrl, od.Product.Price))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<OrderDetailResponse>>.Success(details);
    }
}

using Microsoft.EntityFrameworkCore;
using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Admin;
using WatchStoreApi.Application.DTOs.Orders;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Application.Interfaces.Persistence;
using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Application.Services;

public class AdminService(IAppDbContext dbContext) : IAdminService
{
    public async Task<DashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var totalOrders = await dbContext.Orders.CountAsync(cancellationToken);
        var pendingOrders = await dbContext.Orders.CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
        var totalRevenue = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0;
        var totalProducts = await dbContext.Products.CountAsync(cancellationToken);
        var totalCategories = await dbContext.Categories.CountAsync(cancellationToken);

        return new DashboardResponse(totalOrders, pendingOrders, totalRevenue, totalProducts, totalCategories);
    }

    public async Task<IReadOnlyList<RevenueResponse>> GetRevenueAsync(RevenueRange range, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var result = new List<RevenueResponse>(7);

        for (var i = 6; i >= 0; i--)
        {
            var (start, end, period) = GetPeriod(range, now, i);

            var revenue = await dbContext.Orders
                .Where(o => o.Status == OrderStatus.Delivered && o.OrderDate >= start && o.OrderDate < end)
                .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0;

            result.Add(new RevenueResponse(period, revenue));
        }

        return result;
    }

    public Task<PagedResponse<AdminOrderResponse>> GetAllOrdersAsync(
        int pageNumber,
        int pageSize,
        string? status,
        DateTime? startDate,
        DateTime? endDate,
        string? user,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            query = query.Where(o => o.Status == orderStatus);

        if (startDate.HasValue)
            query = query.Where(o => o.OrderDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.OrderDate <= endDate.Value);

        if (!string.IsNullOrEmpty(user))
            query = query.Where(o => o.User!.Name.Contains(user) || o.User!.Email.Contains(user));

        return query
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new AdminOrderResponse(
                o.Id, o.User!.Name, o.OrderDate, o.TotalAmount, o.Status, o.Address))
            .ToPagedResponseAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<PagedResponse<AdminOrderResponse>> GetPendingOrdersAsync(PagedRequest paging, CancellationToken cancellationToken = default)
    {
        return dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.Pending)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new AdminOrderResponse(
                o.Id, o.User!.Name, o.OrderDate, o.TotalAmount, o.Status, o.Address))
            .ToPagedResponseAsync(paging.PageNumber, paging.PageSize, cancellationToken);
    }

    public async Task<Result<IReadOnlyList<AdminOrderDetailResponse>>> GetOrderDetailsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var orderExists = await dbContext.Orders.AnyAsync(o => o.Id == orderId, cancellationToken);
        if (!orderExists)
            return Result<IReadOnlyList<AdminOrderDetailResponse>>.NotFound("Order not found.");

        var details = await dbContext.OrderDetails
            .AsNoTracking()
            .Where(od => od.OrderId == orderId)
            .Select(od => new AdminOrderDetailResponse(
                od.Id, od.Qty, od.TotalAmount, od.ProductId,
                od.Product!.Name, od.Product.ImageUrl, od.Product.Price))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AdminOrderDetailResponse>>.Success(details);
    }

    public async Task<Result> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders.FindAsync([orderId], cancellationToken);
        if (order == null)
            return Result.NotFound("Order not found.");

        if (!OrderStatusMachine.CanTransition(order.Status, request.NewStatus))
            return Result.Failure($"Cannot transition from '{order.Status}' to '{request.NewStatus}'.");

        order.Status = request.NewStatus;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static (DateTime Start, DateTime End, string Period) GetPeriod(RevenueRange range, DateTime now, int offset)
    {
        switch (range)
        {
            case RevenueRange.Yearly:
                var year = now.Year - offset;
                var yStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return (yStart, yStart.AddYears(1), year.ToString());

            case RevenueRange.Monthly:
                var date = now.AddMonths(-offset);
                var mStart = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                return (mStart, mStart.AddMonths(1), $"{date.Year}-{date.Month:D2}");

            case RevenueRange.Weekly:
                var weekStart = now.Date.AddDays(-7 * offset);
                var wStart = DateTime.SpecifyKind(weekStart.AddDays(-(int)weekStart.DayOfWeek), DateTimeKind.Utc);
                return (wStart, wStart.AddDays(7), wStart.ToString("yyyy-MM-dd"));

            default:
                throw new ArgumentOutOfRangeException(nameof(range), range, null);
        }
    }
}

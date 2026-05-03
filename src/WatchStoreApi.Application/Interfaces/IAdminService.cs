using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Admin;
using WatchStoreApi.Application.DTOs.Orders;
using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Application.Interfaces;

public interface IAdminService
{
    Task<DashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RevenueResponse>> GetRevenueAsync(RevenueRange range, CancellationToken cancellationToken = default);
    Task<PagedResponse<AdminOrderResponse>> GetAllOrdersAsync(
        int pageNumber,
        int pageSize,
        string? status,
        DateTime? startDate,
        DateTime? endDate,
        string? user,
        CancellationToken cancellationToken = default);
    Task<PagedResponse<AdminOrderResponse>> GetPendingOrdersAsync(PagedRequest paging, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AdminOrderDetailResponse>>> GetOrderDetailsAsync(int orderId, CancellationToken cancellationToken = default);
    Task<Result> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
}

using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Orders;

namespace WatchStoreApi.Application.Interfaces;

public interface IOrderService
{
    Task<Result<int>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<OrderSummaryResponse>> GetMyOrdersAsync(PagedRequest paging, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<OrderDetailResponse>>> GetMyOrderDetailsAsync(int orderId, CancellationToken cancellationToken = default);
}

using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Application.Services;

public static class OrderStatusMachine
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> Transitions = new()
    {
        [OrderStatus.Pending] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Processing, OrderStatus.Cancelled],
        [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Cancelled],
        [OrderStatus.Shipped] = [OrderStatus.Delivered],
        [OrderStatus.Delivered] = [OrderStatus.Refunded],
        [OrderStatus.Cancelled] = [],
        [OrderStatus.Refunded] = []
    };

    public static bool CanTransition(OrderStatus from, OrderStatus to)
        => Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
}

namespace WatchStoreApi.Application.DTOs.Admin;

public record DashboardResponse(
    int TotalOrders,
    int PendingOrders,
    decimal TotalRevenue,
    int TotalProducts,
    int TotalCategories
);

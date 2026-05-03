using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Application.DTOs.Orders;

public record UpdateOrderStatusRequest(OrderStatus NewStatus);

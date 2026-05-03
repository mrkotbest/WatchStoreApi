using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Orders;
using WatchStoreApi.Application.Interfaces;

namespace WatchStoreApi.Api.Controllers;

[Route("api/orders")]
[ApiController]
[Authorize]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await orderService.CreateOrderAsync(request, cancellationToken);
        return result.IsSuccess
            ? Created(string.Empty, new { orderId = result.Value })
            : Problem(result.Error, statusCode: result.StatusCode);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders([FromQuery] PagedRequest paging, CancellationToken cancellationToken)
    {
        var orders = await orderService.GetMyOrdersAsync(paging, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetOrderDetails(int orderId, CancellationToken cancellationToken)
    {
        var result = await orderService.GetMyOrderDetailsAsync(orderId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error, statusCode: result.StatusCode);
    }
}

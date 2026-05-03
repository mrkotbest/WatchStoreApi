using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Orders;
using WatchStoreApi.Application.Interfaces;

namespace WatchStoreApi.Api.Controllers.Admin;

[Route("api/admin/orders")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminOrdersController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? user = null,
        CancellationToken cancellationToken = default)
    {
        var orders = await adminService.GetAllOrdersAsync(
            pageNumber, pageSize, status, startDate, endDate, user, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending([FromQuery] PagedRequest paging, CancellationToken cancellationToken)
    {
        var orders = await adminService.GetPendingOrdersAsync(paging, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetDetails(int orderId, CancellationToken cancellationToken)
    {
        var result = await adminService.GetOrderDetailsAsync(orderId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error, statusCode: result.StatusCode);
    }

    [HttpPut("{orderId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await adminService.UpdateOrderStatusAsync(orderId, request, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : Problem(result.Error, statusCode: result.StatusCode);
    }
}

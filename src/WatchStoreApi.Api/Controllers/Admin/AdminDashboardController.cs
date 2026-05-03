using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Api.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminDashboardController(IAdminService adminService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await adminService.GetDashboardAsync(cancellationToken);
        return Ok(dashboard);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] RevenueRange range = RevenueRange.Monthly, CancellationToken cancellationToken = default)
    {
        var revenue = await adminService.GetRevenueAsync(range, cancellationToken);
        return Ok(revenue);
    }
}

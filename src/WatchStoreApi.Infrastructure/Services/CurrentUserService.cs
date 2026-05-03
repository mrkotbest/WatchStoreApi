using Microsoft.AspNetCore.Http;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Infrastructure.Extensions;

namespace WatchStoreApi.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public int UserId => httpContextAccessor.HttpContext?.User.GetUserId() ?? 0;

    public string Email => httpContextAccessor.HttpContext?.User.GetUserEmail() ?? string.Empty;

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}

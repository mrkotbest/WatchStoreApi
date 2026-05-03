using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Auth;

namespace WatchStoreApi.Application.Interfaces;

public interface IAuthService
{
    Task<Result<int>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<LoginResponse>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);
}

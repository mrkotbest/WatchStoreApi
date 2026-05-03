namespace WatchStoreApi.Application.DTOs.Auth;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn,
    int UserId,
    string UserName
);

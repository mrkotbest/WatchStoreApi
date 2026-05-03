using WatchStoreApi.Domain.Entities;

namespace WatchStoreApi.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashToken(string token);
}

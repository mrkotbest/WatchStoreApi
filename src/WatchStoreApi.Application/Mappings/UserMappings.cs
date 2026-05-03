using WatchStoreApi.Application.DTOs.Auth;
using WatchStoreApi.Domain.Entities;
using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Application.Mappings;

public static class UserMappings
{
    public static User ToEntity(this RegisterRequest request) =>
        new()
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };
}

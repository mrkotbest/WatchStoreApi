using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WatchStoreApi.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(sub, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
    }

    public static string GetUserEmail(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
}

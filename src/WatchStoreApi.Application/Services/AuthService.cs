using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Auth;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Application.Interfaces.Persistence;
using WatchStoreApi.Application.Mappings;
using WatchStoreApi.Application.Options;
using WatchStoreApi.Domain.Entities;

namespace WatchStoreApi.Application.Services;

public class AuthService(
    IAppDbContext dbContext,
    ITokenService tokenService,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task<Result<int>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (exists)
            return Result<int>.Failure("User with this email already exists.");

        var user = request.ToEntity();
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<int>.Created(user.Id);
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null)
            return Result<LoginResponse>.NotFound("Invalid email or password.");

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
            return Result<LoginResponse>.NotFound("Invalid email or password.");

        var response = await IssueTokensAsync(user, cancellationToken);
        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result<LoginResponse>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hashedToken = tokenService.HashToken(refreshToken);

        var storedToken = await dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == hashedToken, cancellationToken);

        if (storedToken == null || !storedToken.IsActive)
            return Result<LoginResponse>.Unauthorized("Invalid or expired refresh token.");

        storedToken.RevokedAt = DateTime.UtcNow;

        var response = await IssueTokensAsync(storedToken.User!, cancellationToken,
            replacedTokenLink: stored => storedToken.ReplacedByToken = stored);

        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result> RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hashedToken = tokenService.HashToken(refreshToken);

        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == hashedToken, cancellationToken);

        if (storedToken == null || !storedToken.IsActive)
            return Result.Failure("Token not found or already revoked.");

        storedToken.RevokedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<LoginResponse> IssueTokensAsync(
        User user,
        CancellationToken cancellationToken,
        Action<string>? replacedTokenLink = null)
    {
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        var hashedToken = tokenService.HashToken(refreshToken);

        replacedTokenLink?.Invoke(hashedToken);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Token = hashedToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            accessToken,
            refreshToken,
            "bearer",
            _jwt.AccessTokenExpirationMinutes * 60,
            user.Id,
            user.Name);
    }
}

using Microsoft.Extensions.Options;
using Moq;
using WatchStoreApi.Application.DTOs.Auth;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Application.Options;
using WatchStoreApi.Application.Services;
using WatchStoreApi.Domain.Entities;
using WatchStoreApi.Domain.Enums;
using WatchStoreApi.Infrastructure.Persistence;

namespace WatchStoreApi.Tests.Unit.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AuthService _sut;
    private readonly Mock<ITokenService> _tokenServiceMock;

    public AuthServiceTests()
    {
        _db = DbContextFactory.Create();
        _tokenServiceMock = new Mock<ITokenService>();
        var jwtOptions = new JwtOptions
        {
            Key = "SuperSecretKeyThatIsAtLeast32CharsLong!",
            Issuer = "Test",
            Audience = "Test",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("test-access-token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("test-refresh-token");
        _tokenServiceMock.Setup(x => x.HashToken(It.IsAny<string>()))
            .Returns((string t) => $"hashed-{t}");

        _sut = new AuthService(_db, _tokenServiceMock.Object, Options.Create(jwtOptions));
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreatedWithUserId()
    {
        var request = new RegisterRequest("Test User", "test@example.com", null, "Password123");

        var result = await _sut.RegisterAsync(request);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value > 0);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsFailure()
    {
        var request = new RegisterRequest("User", "dup@example.com", null, "Password123");
        await _sut.RegisterAsync(request);

        var result = await _sut.RegisterAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Contains("already exists", result.Error);
    }

    [Fact]
    public async Task Register_AlwaysAssignsUserRole()
    {
        var request = new RegisterRequest("Admin Imposter", "admin@hack.com", null, "Password123");

        await _sut.RegisterAsync(request);

        var user = _db.Users.First(u => u.Email == "admin@hack.com");
        Assert.Equal(UserRole.User, user.Role);
    }

    [Fact]
    public async Task Register_HashesPassword()
    {
        var request = new RegisterRequest("User", "hash@test.com", null, "MyPassword");

        await _sut.RegisterAsync(request);

        var user = _db.Users.First(u => u.Email == "hash@test.com");
        Assert.NotEqual("MyPassword", user.PasswordHash);
        Assert.NotEmpty(user.PasswordHash);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        await _sut.RegisterAsync(new RegisterRequest("User", "login@test.com", null, "Password123"));

        var result = await _sut.LoginAsync(new LoginRequest("login@test.com", "Password123"));

        Assert.True(result.IsSuccess);
        Assert.Equal("test-access-token", result.Value!.AccessToken);
        Assert.Equal("test-refresh-token", result.Value.RefreshToken);
        Assert.Equal("bearer", result.Value.TokenType);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsNotFound()
    {
        await _sut.RegisterAsync(new RegisterRequest("User", "wrong@test.com", null, "CorrectPass"));

        var result = await _sut.LoginAsync(new LoginRequest("wrong@test.com", "WrongPass"));

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsNotFound()
    {
        var result = await _sut.LoginAsync(new LoginRequest("nobody@test.com", "Pass"));

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task Login_SavesRefreshTokenInDb()
    {
        await _sut.RegisterAsync(new RegisterRequest("User", "refresh@test.com", null, "Password123"));

        await _sut.LoginAsync(new LoginRequest("refresh@test.com", "Password123"));

        var tokens = _db.RefreshTokens.Where(rt => rt.Token == "hashed-test-refresh-token").ToList();
        Assert.Single(tokens);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        await _sut.RegisterAsync(new RegisterRequest("User", "ref@test.com", null, "Pass1234"));
        await _sut.LoginAsync(new LoginRequest("ref@test.com", "Pass1234"));

        var result = await _sut.RefreshAsync("test-refresh-token");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task Refresh_RevokesOldToken()
    {
        await _sut.RegisterAsync(new RegisterRequest("User", "revoke@test.com", null, "Pass1234"));
        await _sut.LoginAsync(new LoginRequest("revoke@test.com", "Pass1234"));

        await _sut.RefreshAsync("test-refresh-token");

        var oldToken = _db.RefreshTokens.First(rt => rt.Token == "hashed-test-refresh-token");
        Assert.NotNull(oldToken.RevokedAt);
    }

    [Fact]
    public async Task Revoke_WithValidToken_MarksAsRevoked()
    {
        await _sut.RegisterAsync(new RegisterRequest("User", "rev2@test.com", null, "Pass1234"));
        await _sut.LoginAsync(new LoginRequest("rev2@test.com", "Pass1234"));

        var result = await _sut.RevokeAsync("test-refresh-token");

        Assert.True(result.IsSuccess);
        var token = _db.RefreshTokens.First(rt => rt.Token == "hashed-test-refresh-token");
        Assert.NotNull(token.RevokedAt);
    }

    [Fact]
    public async Task Revoke_WithInvalidToken_ReturnsFailure()
    {
        var result = await _sut.RevokeAsync("nonexistent-token");

        Assert.False(result.IsSuccess);
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }
}

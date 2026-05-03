using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WatchStoreApi.Application.DTOs.Auth;

namespace WatchStoreApi.Tests.Integration;

public class AuthIntegrationTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Register_ValidUser_Returns201()
    {
        using var client = factory.CreateClient();
        var request = new RegisterRequest("IntTest", $"reg-{Guid.NewGuid()}@test.com", null, "Password123");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        using var client = factory.CreateClient();
        var email = $"dup-{Guid.NewGuid()}@test.com";
        var request = new RegisterRequest("Dup", email, null, "Password123");
        await client.PostAsJsonAsync("/api/auth/register", request);

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        using var client = factory.CreateClient();
        var email = $"login-{Guid.NewGuid()}@test.com";
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("User", email, null, "Password123"));

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, "Password123"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body.AccessToken);
        Assert.NotEmpty(body.RefreshToken);
        Assert.Equal("bearer", body.TokenType);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns404()
    {
        using var client = factory.CreateClient();
        var email = $"wrong-{Guid.NewGuid()}@test.com";
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("User", email, null, "Password123"));

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, "WrongPass"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/cart");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithToken_Returns200()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/cart");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_WithUserRole_Returns403()
    {
        using var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/admin/dashboard");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewTokens()
    {
        using var client = factory.CreateClient();
        var email = $"refresh-{Guid.NewGuid()}@test.com";
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("User", email, null, "Password123"));
        var loginResp = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, "Password123"));
        var tokens = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

        var response = await client.PostAsJsonAsync("/api/auth/refresh",
            new { RefreshToken = tokens!.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var newTokens = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotEqual(tokens.AccessToken, newTokens!.AccessToken);
    }

    [Fact]
    public async Task Revoke_ValidToken_ReturnsOk()
    {
        using var client = factory.CreateClient();
        var (email, tokens) = await RegisterAndLoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await client.PostAsJsonAsync("/api/auth/revoke",
            new { tokens.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var refreshAttempt = await client.PostAsJsonAsync("/api/auth/refresh",
            new { tokens.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refreshAttempt.StatusCode);
        _ = email;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = factory.CreateClient();
        var (_, tokens) = await RegisterAndLoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        return client;
    }

    private static async Task<(string email, LoginResponse tokens)> RegisterAndLoginAsync(HttpClient client)
    {
        var email = $"user-{Guid.NewGuid()}@test.com";
        await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("User", email, null, "Password123"));
        var loginResp = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, "Password123"));
        var tokens = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();
        return (email, tokens!);
    }
}

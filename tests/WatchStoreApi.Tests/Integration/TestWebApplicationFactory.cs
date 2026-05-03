using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WatchStoreApi.Application.Interfaces.Persistence;
using WatchStoreApi.Infrastructure.Persistence;

namespace WatchStoreApi.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("Jwt__Key", "TestSuperSecretKeyThatIsAtLeast32CharsLong!");
        Environment.SetEnvironmentVariable("RateLimiting__LoginPermitLimit", "100000");
        Environment.SetEnvironmentVariable("RateLimiting__LoginWindowSeconds", "1");
        Environment.SetEnvironmentVariable("RateLimiting__GeneralPermitLimit", "100000");
        Environment.SetEnvironmentVariable("RateLimiting__GeneralWindowSeconds", "1");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestSuperSecretKeyThatIsAtLeast32CharsLong!",
                ["Jwt:Issuer"] = "WatchStoreApi",
                ["Jwt:Audience"] = "WatchStoreApiUsers",
                ["Jwt:AccessTokenExpirationMinutes"] = "15",
                ["Jwt:RefreshTokenExpirationDays"] = "7",
                ["ConnectionStrings:DefaultConnection"] = "",
                ["RateLimiting:LoginPermitLimit"] = "100000",
                ["RateLimiting:LoginWindowSeconds"] = "1",
                ["RateLimiting:GeneralPermitLimit"] = "100000",
                ["RateLimiting:GeneralWindowSeconds"] = "1"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                         || d.ServiceType == typeof(DbContextOptions)
                         || d.ServiceType == typeof(IAppDbContext)
                         || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                .ToList();
            foreach (var d in descriptors) services.Remove(d);

            var dbName = "IntegrationTestDb_" + Guid.NewGuid();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName)
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Development");
    }
}

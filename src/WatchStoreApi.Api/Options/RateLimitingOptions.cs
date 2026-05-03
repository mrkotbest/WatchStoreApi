namespace WatchStoreApi.Api.Options;

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int LoginPermitLimit { get; set; } = 5;
    public int LoginWindowSeconds { get; set; } = 300;
    public int GeneralPermitLimit { get; set; } = 100;
    public int GeneralWindowSeconds { get; set; } = 60;
}

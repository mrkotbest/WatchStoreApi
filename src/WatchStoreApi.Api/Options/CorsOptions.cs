namespace WatchStoreApi.Api.Options;

public class WatchStoreCorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = [];
}

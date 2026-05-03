namespace WatchStoreApi.Application.Options;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public long MaxFileSizeBytes { get; set; } = 5_242_880;
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
    public string UploadPath { get; set; } = "images/products";
}

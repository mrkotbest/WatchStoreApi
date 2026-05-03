using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Application.Options;

namespace WatchStoreApi.Infrastructure.Services;

public class FileService(IWebHostEnvironment environment, IOptions<FileStorageOptions> options) : IFileService
{
    private readonly FileStorageOptions _options = options.Value;

    private static readonly Dictionary<string, byte[]> MagicBytes = new()
    {
        [".jpg"] = [0xFF, 0xD8, 0xFF],
        [".jpeg"] = [0xFF, 0xD8, 0xFF],
        [".png"] = [0x89, 0x50, 0x4E, 0x47],
        [".webp"] = [0x52, 0x49, 0x46, 0x46]
    };

    public async Task<Result<string>> SaveImageAsync(ProductImage image, CancellationToken cancellationToken = default)
    {
        if (image.Length > _options.MaxFileSizeBytes)
            return Result<string>.Failure($"File size exceeds the limit of {_options.MaxFileSizeBytes / 1024 / 1024}MB.");

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension))
            return Result<string>.Failure($"File type '{extension}' is not allowed.");

        if (!await ValidateMagicBytesAsync(image.Content, extension, cancellationToken))
            return Result<string>.Failure("File content does not match the expected format.");

        var uploadPath = Path.Combine(environment.WebRootPath, _options.UploadPath);
        Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        if (image.Content.CanSeek)
            image.Content.Position = 0;
        await image.Content.CopyToAsync(fileStream, cancellationToken);

        return Result<string>.Success(fileName);
    }

    public void DeleteImage(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return;

        var filePath = Path.Combine(environment.WebRootPath, _options.UploadPath, fileName);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    private static async Task<bool> ValidateMagicBytesAsync(Stream content, string extension, CancellationToken cancellationToken)
    {
        if (!MagicBytes.TryGetValue(extension, out var expectedBytes))
            return false;

        if (content.CanSeek)
            content.Position = 0;

        var buffer = new byte[expectedBytes.Length];
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            var read = await content.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead), cancellationToken);
            if (read == 0)
                return false;
            totalRead += read;
        }

        return buffer.AsSpan().SequenceEqual(expectedBytes);
    }
}

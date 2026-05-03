using WatchStoreApi.Application.Common;

namespace WatchStoreApi.Application.Interfaces;

public interface IFileService
{
    Task<Result<string>> SaveImageAsync(ProductImage image, CancellationToken cancellationToken = default);
    void DeleteImage(string? fileName);
}

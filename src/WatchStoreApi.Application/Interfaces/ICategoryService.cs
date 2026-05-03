using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Categories;

namespace WatchStoreApi.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<int>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

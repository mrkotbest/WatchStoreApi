using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Products;

namespace WatchStoreApi.Application.Interfaces;

public interface IProductService
{
    Task<PagedResponse<ProductResponse>> GetAllAsync(ProductFilterRequest filter, CancellationToken cancellationToken = default);
    Task<Result<ProductResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<int>> CreateAsync(CreateProductRequest request, ProductImage? image, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(int id, UpdateProductRequest request, ProductImage? image, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

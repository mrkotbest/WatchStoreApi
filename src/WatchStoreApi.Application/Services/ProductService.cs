using Microsoft.EntityFrameworkCore;
using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Products;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Application.Interfaces.Persistence;
using WatchStoreApi.Application.Mappings;

namespace WatchStoreApi.Application.Services;

public class ProductService(IAppDbContext dbContext, IFileService fileService) : IProductService
{
    public Task<PagedResponse<ProductResponse>> GetAllAsync(ProductFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Products.AsNoTracking();

        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(p => p.Name.Contains(filter.Search) || p.Description.Contains(filter.Search));

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (!string.IsNullOrEmpty(filter.Material))
            query = query.Where(p => p.Material == filter.Material);

        if (filter.Gender.HasValue)
            query = query.Where(p => p.Gender == filter.Gender.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        return query
            .OrderBy(p => p.Id)
            .Select(p => new ProductResponse(
                p.Id, p.Name, p.Description, p.ImageUrl,
                p.Material, p.Gender, p.Price, p.CategoryId))
            .ToPagedResponseAsync(filter.PageNumber, filter.PageSize, cancellationToken);
    }

    public async Task<Result<ProductResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products.FindAsync([id], cancellationToken);
        return product == null
            ? Result<ProductResponse>.NotFound("Product not found.")
            : Result<ProductResponse>.Success(product.ToResponse());
    }

    public async Task<Result<int>> CreateAsync(CreateProductRequest request, ProductImage? image, CancellationToken cancellationToken = default)
    {
        if (image == null)
            return Result<int>.Failure("Product image is required.");

        var imageResult = await fileService.SaveImageAsync(image, cancellationToken);
        if (!imageResult.IsSuccess)
            return Result<int>.Failure(imageResult.Error!);

        var product = request.ToEntity(imageResult.Value);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<int>.Created(product.Id);
    }

    public async Task<Result> UpdateAsync(int id, UpdateProductRequest request, ProductImage? image, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products.FindAsync([id], cancellationToken);
        if (product == null)
            return Result.NotFound("Product not found.");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Material = request.Material;
        product.Gender = request.Gender;
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;

        if (image != null)
        {
            var imageResult = await fileService.SaveImageAsync(image, cancellationToken);
            if (!imageResult.IsSuccess)
                return Result.Failure(imageResult.Error!);

            var oldImage = product.ImageUrl;
            product.ImageUrl = imageResult.Value;
            fileService.DeleteImage(oldImage);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products.FindAsync([id], cancellationToken);
        if (product == null)
            return Result.NotFound("Product not found.");

        var imageUrl = product.ImageUrl;
        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        fileService.DeleteImage(imageUrl);

        return Result.Success();
    }
}

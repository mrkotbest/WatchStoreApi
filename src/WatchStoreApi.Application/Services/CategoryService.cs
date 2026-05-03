using Microsoft.EntityFrameworkCore;
using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Categories;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Application.Interfaces.Persistence;
using WatchStoreApi.Application.Mappings;

namespace WatchStoreApi.Application.Services;

public class CategoryService(IAppDbContext dbContext) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Select(c => new CategoryResponse(c.Id, c.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<int>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = request.ToEntity();
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<int>.Created(category.Id);
    }

    public async Task<Result> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await dbContext.Categories.FindAsync([id], cancellationToken);
        if (category == null)
            return Result.NotFound("Category not found.");

        category.Name = request.Name;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await dbContext.Categories.FindAsync([id], cancellationToken);
        if (category == null)
            return Result.NotFound("Category not found.");

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using WatchStoreApi.Application.DTOs.Categories;
using WatchStoreApi.Application.Services;
using WatchStoreApi.Domain.Entities;
using WatchStoreApi.Infrastructure.Persistence;

namespace WatchStoreApi.Tests.Unit.Services;

public class CategoryServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _db = DbContextFactory.Create();
        _sut = new CategoryService(_db);
    }

    [Fact]
    public async Task GetAll_ReturnsSeedCategories()
    {
        var result = await _sut.GetAllAsync();

        Assert.True(result.Count >= 4);
    }

    [Fact]
    public async Task Create_AddsCategory()
    {
        var result = await _sut.CreateAsync(new CreateCategoryRequest("New Category"));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value > 0);
        Assert.NotNull(_db.Categories.FirstOrDefault(c => c.Name == "New Category"));
    }

    [Fact]
    public async Task Update_ExistingCategory_Succeeds()
    {
        _db.Categories.Add(new Category { Name = "Old" });
        await _db.SaveChangesAsync();
        var id = _db.Categories.First(c => c.Name == "Old").Id;

        var result = await _sut.UpdateAsync(id, new UpdateCategoryRequest("Updated"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", _db.Categories.Find(id)!.Name);
    }

    [Fact]
    public async Task Update_NonExistent_ReturnsNotFound()
    {
        var result = await _sut.UpdateAsync(9999, new UpdateCategoryRequest("X"));

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingCategory_Succeeds()
    {
        _db.Categories.Add(new Category { Name = "ToDelete" });
        await _db.SaveChangesAsync();
        var id = _db.Categories.First(c => c.Name == "ToDelete").Id;

        var result = await _sut.DeleteAsync(id);

        Assert.True(result.IsSuccess);
        Assert.Null(_db.Categories.Find(id));
    }

    [Fact]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var result = await _sut.DeleteAsync(9999);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }
}

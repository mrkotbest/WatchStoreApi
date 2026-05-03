using Moq;
using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Products;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Application.Services;
using WatchStoreApi.Domain.Entities;
using WatchStoreApi.Domain.Enums;
using WatchStoreApi.Infrastructure.Persistence;

namespace WatchStoreApi.Tests.Unit.Services;

public class ProductServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ProductService _sut;
    private readonly Mock<IFileService> _fileServiceMock;

    public ProductServiceTests()
    {
        _db = DbContextFactory.Create();
        _fileServiceMock = new Mock<IFileService>();
        _fileServiceMock
            .Setup(x => x.SaveImageAsync(It.IsAny<ProductImage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("test-image.jpg"));
        _sut = new ProductService(_db, _fileServiceMock.Object);
    }

    private static ProductImage CreateImage(string fileName = "test.jpg")
    {
        var stream = new MemoryStream([0xFF, 0xD8, 0xFF]);
        return new ProductImage(stream, fileName, stream.Length);
    }

    [Fact]
    public async Task GetAll_ReturnsSeedProducts()
    {
        var filter = new ProductFilterRequest { PageNumber = 1, PageSize = 50 };

        var result = await _sut.GetAllAsync(filter);

        Assert.True(result.TotalCount >= 20);
        Assert.True(result.Items.Count > 0);
    }

    [Fact]
    public async Task GetAll_FilterByCategory_ReturnsFilteredProducts()
    {
        var filter = new ProductFilterRequest { CategoryId = 1, PageSize = 50 };

        var result = await _sut.GetAllAsync(filter);

        Assert.All(result.Items, p => Assert.Equal(1, p.CategoryId));
    }

    [Fact]
    public async Task GetAll_FilterByGender_Works()
    {
        var filter = new ProductFilterRequest { Gender = Gender.Female, PageSize = 50 };

        var result = await _sut.GetAllAsync(filter);

        Assert.All(result.Items, p => Assert.Equal(Gender.Female, p.Gender));
    }

    [Fact]
    public async Task GetAll_FilterByPriceRange_Works()
    {
        var filter = new ProductFilterRequest { MinPrice = 500, MaxPrice = 700, PageSize = 50 };

        var result = await _sut.GetAllAsync(filter);

        Assert.All(result.Items, p =>
        {
            Assert.True(p.Price >= 500);
            Assert.True(p.Price <= 700);
        });
    }

    [Fact]
    public async Task GetAll_Search_FindsByName()
    {
        var filter = new ProductFilterRequest { Search = "Doroly", PageSize = 50 };

        var result = await _sut.GetAllAsync(filter);

        Assert.Contains(result.Items, p => p.Name == "Doroly");
    }

    [Fact]
    public async Task GetAll_Pagination_ReturnsCorrectPage()
    {
        var page1 = await _sut.GetAllAsync(new ProductFilterRequest { PageNumber = 1, PageSize = 5 });
        var page2 = await _sut.GetAllAsync(new ProductFilterRequest { PageNumber = 2, PageSize = 5 });

        Assert.Equal(5, page1.Items.Count);
        Assert.Equal(5, page2.Items.Count);
        Assert.DoesNotContain(page2.Items, p => page1.Items.Any(p1 => p1.Id == p.Id));
    }

    [Fact]
    public async Task GetAll_PaginationMetadata_IsCorrect()
    {
        var result = await _sut.GetAllAsync(new ProductFilterRequest { PageNumber = 1, PageSize = 5 });

        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.True(result.TotalCount >= 20);
        Assert.True(result.TotalPages >= 4);
        Assert.False(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task GetById_ExistingProduct_ReturnsProduct()
    {
        var result = await _sut.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.Equal("Doroly", result.Value!.Name);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var result = await _sut.GetByIdAsync(9999);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutImage_ReturnsFailure()
    {
        var request = new CreateProductRequest
        {
            Name = "X", Description = "Y", Material = "Z",
            Gender = Gender.Male, Price = 1, CategoryId = 1
        };

        var result = await _sut.CreateAsync(request, image: null);

        Assert.False(result.IsSuccess);
        Assert.Contains("image", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WithImage_PersistsProduct()
    {
        var request = new CreateProductRequest
        {
            Name = "Created", Description = "Y", Material = "Z",
            Gender = Gender.Male, Price = 100, CategoryId = 1
        };
        var image = CreateImage();

        var result = await _sut.CreateAsync(request, image);

        Assert.True(result.IsSuccess);
        var product = _db.Products.Find(result.Value);
        Assert.NotNull(product);
        Assert.Equal("test-image.jpg", product.ImageUrl);
    }

    [Fact]
    public async Task Update_ExistingProduct_Succeeds()
    {
        _db.Products.Add(new Product
        {
            Name = "ToUpdate", Description = "D", Material = "S",
            Gender = Gender.Male, Price = 100, CategoryId = 1
        });
        await _db.SaveChangesAsync();
        var id = _db.Products.First(p => p.Name == "ToUpdate").Id;

        var request = new UpdateProductRequest
        {
            Name = "Updated", Description = "D2", Material = "S",
            Gender = Gender.Male, Price = 200, CategoryId = 1
        };

        var result = await _sut.UpdateAsync(id, request, image: null);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", _db.Products.Find(id)!.Name);
    }

    [Fact]
    public async Task Delete_ExistingProduct_Succeeds()
    {
        _db.Products.Add(new Product
        {
            Name = "ToDelete", Description = "D", Material = "S",
            Gender = Gender.Male, Price = 100, CategoryId = 1
        });
        await _db.SaveChangesAsync();
        var id = _db.Products.First(p => p.Name == "ToDelete").Id;

        var result = await _sut.DeleteAsync(id);

        Assert.True(result.IsSuccess);
        Assert.Null(_db.Products.Find(id));
    }

    [Fact]
    public async Task Delete_CallsFileServiceDeleteImage()
    {
        _db.Products.Add(new Product
        {
            Name = "WithImg", Description = "D", Material = "S",
            Gender = Gender.Male, Price = 100, CategoryId = 1, ImageUrl = "img.jpg"
        });
        await _db.SaveChangesAsync();
        var id = _db.Products.First(p => p.Name == "WithImg").Id;

        await _sut.DeleteAsync(id);

        _fileServiceMock.Verify(x => x.DeleteImage("img.jpg"), Times.Once);
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }
}

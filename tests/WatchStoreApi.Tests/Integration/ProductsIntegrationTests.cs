using System.Net;
using System.Net.Http.Json;
using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Products;

namespace WatchStoreApi.Tests.Integration;

public class ProductsIntegrationTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetProducts_NoAuth_Returns200()
    {
        var response = await _client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_ReturnsPaginatedResponse()
    {
        var response = await _client.GetFromJsonAsync<PagedResponse<ProductResponse>>(
            "/api/products?pageNumber=1&pageSize=5");

        Assert.NotNull(response);
        Assert.True(response.Items.Count <= 5);
        Assert.True(response.TotalCount > 0);
        Assert.True(response.TotalPages > 0);
    }

    [Fact]
    public async Task GetProducts_FilterByCategory_Works()
    {
        var response = await _client.GetFromJsonAsync<PagedResponse<ProductResponse>>(
            "/api/products?categoryId=1&pageSize=50");

        Assert.NotNull(response);
        Assert.All(response.Items, p => Assert.Equal(1, p.CategoryId));
    }

    [Fact]
    public async Task GetProducts_FilterByPriceRange_Works()
    {
        var response = await _client.GetFromJsonAsync<PagedResponse<ProductResponse>>(
            "/api/products?minPrice=500&maxPrice=700&pageSize=50");

        Assert.NotNull(response);
        Assert.All(response.Items, p =>
        {
            Assert.True(p.Price >= 500);
            Assert.True(p.Price <= 700);
        });
    }

    [Fact]
    public async Task GetProductById_Existing_Returns200()
    {
        var response = await _client.GetAsync("/api/products/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(product);
        Assert.Equal(1, product.Id);
    }

    [Fact]
    public async Task GetProductById_NonExistent_Returns404()
    {
        var response = await _client.GetAsync("/api/products/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCategories_Returns200WithSeedData()
    {
        var response = await _client.GetAsync("/api/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Classic", content);
    }

    [Fact]
    public async Task Swagger_IsAvailable()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

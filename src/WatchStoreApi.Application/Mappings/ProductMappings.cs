using WatchStoreApi.Application.DTOs.Products;
using WatchStoreApi.Domain.Entities;

namespace WatchStoreApi.Application.Mappings;

public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product product) =>
        new(product.Id, product.Name, product.Description, product.ImageUrl,
            product.Material, product.Gender, product.Price, product.CategoryId);

    public static Product ToEntity(this CreateProductRequest request, string? imageUrl) =>
        new()
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = imageUrl,
            Material = request.Material,
            Gender = request.Gender,
            Price = request.Price,
            CategoryId = request.CategoryId
        };
}

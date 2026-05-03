using WatchStoreApi.Application.DTOs.Categories;
using WatchStoreApi.Domain.Entities;

namespace WatchStoreApi.Application.Mappings;

public static class CategoryMappings
{
    public static Category ToEntity(this CreateCategoryRequest request) =>
        new() { Name = request.Name };
}

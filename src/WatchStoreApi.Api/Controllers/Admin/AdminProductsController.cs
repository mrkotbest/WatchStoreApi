using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Products;
using WatchStoreApi.Application.Interfaces;

namespace WatchStoreApi.Api.Controllers.Admin;

[Route("api/admin/products")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminProductsController(IProductService productService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateProductRequest request,
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        var productImage = ToProductImage(image);
        try
        {
            var result = await productService.CreateAsync(request, productImage, cancellationToken);
            return result.IsSuccess
                ? Created(string.Empty, new { id = result.Value })
                : Problem(result.Error, statusCode: result.StatusCode);
        }
        finally
        {
            if (productImage != null)
                await productImage.Content.DisposeAsync();
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromForm] UpdateProductRequest request,
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        var productImage = ToProductImage(image);
        try
        {
            var result = await productService.UpdateAsync(id, request, productImage, cancellationToken);
            return result.IsSuccess
                ? Ok()
                : Problem(result.Error, statusCode: result.StatusCode);
        }
        finally
        {
            if (productImage != null)
                await productImage.Content.DisposeAsync();
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await productService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : Problem(result.Error, statusCode: result.StatusCode);
    }

    private static ProductImage? ToProductImage(IFormFile? file) =>
        file is { Length: > 0 }
            ? new ProductImage(file.OpenReadStream(), file.FileName, file.Length)
            : null;
}

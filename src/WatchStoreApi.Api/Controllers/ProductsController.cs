using Microsoft.AspNetCore.Mvc;
using WatchStoreApi.Application.DTOs.Products;
using WatchStoreApi.Application.Interfaces;

namespace WatchStoreApi.Api.Controllers;

[Route("api/products")]
[ApiController]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ProductFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await productService.GetAllAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var result = await productService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error, statusCode: result.StatusCode);
    }
}

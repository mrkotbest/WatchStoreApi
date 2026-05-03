using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchStoreApi.Application.DTOs.Cart;
using WatchStoreApi.Application.Interfaces;

namespace WatchStoreApi.Api.Controllers;

[Route("api/cart")]
[ApiController]
[Authorize]
public class CartController(ICartService cartService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var items = await cartService.GetCartItemsAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddToCartRequest request, CancellationToken cancellationToken)
    {
        var result = await cartService.AddToCartAsync(request, cancellationToken);
        return result.IsSuccess
            ? Created()
            : Problem(result.Error, statusCode: result.StatusCode);
    }

    [HttpPut("{productId:int}")]
    public async Task<IActionResult> Update(int productId, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        var result = await cartService.UpdateCartItemAsync(productId, request, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : Problem(result.Error, statusCode: result.StatusCode);
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove(int productId, CancellationToken cancellationToken)
    {
        var result = await cartService.RemoveFromCartAsync(productId, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : Problem(result.Error, statusCode: result.StatusCode);
    }
}

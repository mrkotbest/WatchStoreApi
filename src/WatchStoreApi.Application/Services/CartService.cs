using Microsoft.EntityFrameworkCore;
using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Cart;
using WatchStoreApi.Application.Interfaces;
using WatchStoreApi.Application.Interfaces.Persistence;
using WatchStoreApi.Domain.Entities;

namespace WatchStoreApi.Application.Services;

public class CartService(IAppDbContext dbContext, ICurrentUserService currentUser) : ICartService
{
    public async Task<IReadOnlyList<CartItemResponse>> GetCartItemsAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        return await dbContext.ShoppingCartItems
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => new CartItemResponse(
                s.Id, s.Qty, s.UnitPrice, s.TotalAmount,
                s.ProductId, s.Product!.Name, s.Product.ImageUrl))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result> AddToCartAsync(AddToCartRequest request, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var product = await dbContext.Products.FindAsync([request.ProductId], cancellationToken);
        if (product == null)
            return Result.NotFound("Product not found.");

        var existingItem = await dbContext.ShoppingCartItems
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId && s.UserId == userId, cancellationToken);

        if (existingItem != null)
        {
            existingItem.Qty += request.Qty;
            existingItem.TotalAmount = existingItem.UnitPrice * existingItem.Qty;
        }
        else
        {
            dbContext.ShoppingCartItems.Add(new ShoppingCartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                Qty = request.Qty,
                UnitPrice = product.Price,
                TotalAmount = product.Price * request.Qty
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> UpdateCartItemAsync(int productId, UpdateCartItemRequest request, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var cartItem = await dbContext.ShoppingCartItems
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.UserId == userId, cancellationToken);

        if (cartItem == null)
            return Result.NotFound("Product not found in the cart.");

        switch (request.Action.ToLowerInvariant())
        {
            case "increase":
                cartItem.Qty++;
                break;
            case "decrease":
                if (cartItem.Qty > 1)
                    cartItem.Qty--;
                else
                    dbContext.ShoppingCartItems.Remove(cartItem);
                break;
            default:
                return Result.Failure("Invalid action. Use 'increase' or 'decrease'.");
        }

        cartItem.TotalAmount = cartItem.UnitPrice * cartItem.Qty;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveFromCartAsync(int productId, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var cartItem = await dbContext.ShoppingCartItems
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.UserId == userId, cancellationToken);

        if (cartItem == null)
            return Result.NotFound("Product not found in the cart.");

        dbContext.ShoppingCartItems.Remove(cartItem);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using WatchStoreApi.Application.Common;
using WatchStoreApi.Application.DTOs.Cart;

namespace WatchStoreApi.Application.Interfaces;

public interface ICartService
{
    Task<IReadOnlyList<CartItemResponse>> GetCartItemsAsync(CancellationToken cancellationToken = default);
    Task<Result> AddToCartAsync(AddToCartRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateCartItemAsync(int productId, UpdateCartItemRequest request, CancellationToken cancellationToken = default);
    Task<Result> RemoveFromCartAsync(int productId, CancellationToken cancellationToken = default);
}

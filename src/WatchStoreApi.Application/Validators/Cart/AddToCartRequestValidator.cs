using FluentValidation;
using WatchStoreApi.Application.DTOs.Cart;

namespace WatchStoreApi.Application.Validators.Cart;

public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Qty).InclusiveBetween(1, 100);
    }
}

using FluentValidation;
using WatchStoreApi.Application.DTOs.Cart;

namespace WatchStoreApi.Application.Validators.Cart;

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    private static readonly string[] AllowedActions = ["increase", "decrease"];

    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .Must(a => a is not null && AllowedActions.Contains(a.ToLowerInvariant()))
            .WithMessage("Action must be 'increase' or 'decrease'.");
    }
}

using FluentValidation;
using WatchStoreApi.Application.DTOs.Orders;

namespace WatchStoreApi.Application.Validators.Orders;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(500);
    }
}

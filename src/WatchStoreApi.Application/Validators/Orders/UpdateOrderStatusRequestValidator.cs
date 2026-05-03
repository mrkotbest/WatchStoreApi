using FluentValidation;
using WatchStoreApi.Application.DTOs.Orders;

namespace WatchStoreApi.Application.Validators.Orders;

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Invalid order status.");
    }
}

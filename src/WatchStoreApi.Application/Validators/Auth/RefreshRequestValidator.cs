using FluentValidation;
using WatchStoreApi.Application.DTOs.Auth;

namespace WatchStoreApi.Application.Validators.Auth;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.")
            .MaximumLength(512);
    }
}

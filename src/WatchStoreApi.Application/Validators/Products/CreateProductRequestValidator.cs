using FluentValidation;
using WatchStoreApi.Application.DTOs.Products;

namespace WatchStoreApi.Application.Validators.Products;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Material).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Gender).IsInEnum();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}

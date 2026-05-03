using FluentValidation;
using WatchStoreApi.Application.DTOs.Categories;

namespace WatchStoreApi.Application.Validators.Categories;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

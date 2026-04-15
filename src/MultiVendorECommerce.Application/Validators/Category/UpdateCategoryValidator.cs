using FluentValidation;
using MultiVendorECommerce.Application.DTOs.Category;

namespace MultiVendorECommerce.Application.Validators.Category;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDTO>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("Category name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Category description must not exceed 500 characters.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid category status.");
    }
}

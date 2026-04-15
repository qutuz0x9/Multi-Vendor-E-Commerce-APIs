using FluentValidation;
using MultiVendorECommerce.Application.DTOs.Category;

namespace MultiVendorECommerce.Application.Validators.Category;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDTO>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MinimumLength(2).WithMessage("Category name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Category description is required.")
            .MaximumLength(500).WithMessage("Category description must not exceed 500 characters.");
    }
}

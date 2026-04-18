using FluentValidation;
using MultiVendorECommerce.Application.DTOs.ProductCategory;

namespace MultiVendorECommerce.Application.Validators.ProductCategory;

public class CreateProductCategoryValidator : AbstractValidator<CreateProductCategoryDTO>
{
    public CreateProductCategoryValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("A valid product is required.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("A valid category is required.");
    }
}

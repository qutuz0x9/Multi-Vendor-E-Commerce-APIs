using FluentValidation;
using MultiVendorECommerce.Application.DTOs.Product;

namespace MultiVendorECommerce.Application.Validators.Product;

public class CreateProductValidator : AbstractValidator<CreateProductDTO>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.BrandId)
            .GreaterThan(0).WithMessage("A valid brand is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MinimumLength(2).WithMessage("Product name must be at least 2 characters long.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description is required.")
            .MaximumLength(2000).WithMessage("Product description must not exceed 2000 characters.");
    }
}

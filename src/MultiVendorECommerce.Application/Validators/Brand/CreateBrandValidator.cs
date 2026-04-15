using FluentValidation;
using MultiVendorECommerce.Application.DTOs.Brand;

namespace MultiVendorECommerce.Application.Validators.Brand;

public class CreateBrandValidator : AbstractValidator<CreateBrandDTO>
{
    public CreateBrandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required.")
            .MinimumLength(2).WithMessage("Brand name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Brand name must not exceed 100 characters.");
    }
}

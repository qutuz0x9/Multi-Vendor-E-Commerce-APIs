using FluentValidation;
using MultiVendorECommerce.Application.DTOs.Brand;

namespace MultiVendorECommerce.Application.Validators.Brand;

public class UpdateBrandValidator : AbstractValidator<UpdateBrandDTO>
{
    public UpdateBrandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required.")
            .MinimumLength(2).WithMessage("Brand name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Brand name must not exceed 100 characters.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid brand status.");
    }
}

using FluentValidation;
using MultiVendorECommerce.Application.DTOs.CartItem;

namespace MultiVendorECommerce.Application.Validators.CartItem;

public class AddCartItemValidator : AbstractValidator<AddCartItemDTO>
{
    public AddCartItemValidator()
    {
        RuleFor(x => x.VendorOfferId)
            .GreaterThan(0).WithMessage("A valid vendor offer must be specified.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}

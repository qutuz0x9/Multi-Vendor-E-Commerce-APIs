using FluentValidation;
using MultiVendorECommerce.Application.DTOs.CartItem;

namespace MultiVendorECommerce.Application.Validators.CartItem;

public class UpdateCartItemValidator : AbstractValidator<UpdateCartItemDTO>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}

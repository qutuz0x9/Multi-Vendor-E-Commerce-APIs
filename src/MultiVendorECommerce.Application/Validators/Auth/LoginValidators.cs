using FluentValidation;
using MultiVendorECommerce.Application.DTOs.Auth;

namespace MultiVendorECommerce.Application.Validators.Auth;

public class LoginValidator : AbstractValidator<LoginRequestDTO>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
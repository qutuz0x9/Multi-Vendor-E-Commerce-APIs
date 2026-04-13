using FluentValidation;
using MultiVendorECommerce.Application.DTOs.Auth;
namespace MultiVendorECommerce.Application.Validators.Auth;

public class RegisterVendorValidator : AbstractValidator<RegisterVendorDTO>
{
    public RegisterVendorValidator()
    {
        // 1) Username: Required, minimum length of 3 characters.
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.");
        // 2) Email: Required, must be a valid email format.
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
        // 3) Password: Required, minimum length of 8 characters, must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");
        // 4) Confirm Password: Required, must match the Password field.
        RuleFor(x => x.PasswordConfirm)
            .NotEmpty().WithMessage("Confirm Password is required.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
        // 5) Phone Number: Required, must be a valid phone number format (e.g., E.164 format).
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format.");
        // 6) Business Name: Required, minimum length of 2 characters.
        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("Business name is required.")
            .MinimumLength(2).WithMessage("Business name must be at least 2 characters long.");
        // 7) Website URL: Required, must be a valid URL format.
        RuleFor(x => x.WebsiteUrl)
            .NotEmpty().WithMessage("Website URL is required.")
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute)).WithMessage("Invalid URL format.");
    }
}
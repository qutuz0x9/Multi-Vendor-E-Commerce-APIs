using FluentAssertions;
using MultiVendorECommerce.Application.DTOs.Auth;
using MultiVendorECommerce.Application.Validators.Auth;

namespace MultiVendorECommerce.Application.Test.ValidatorTest;

public class RegisterVendorValidatorTest
{
    private readonly RegisterVendorValidator _validator;

    public RegisterVendorValidatorTest()
    {
        _validator = new RegisterVendorValidator();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static RegisterVendorDTO ValidDto() => new()
    {
        Username = "testvendor",
        Email = "vendor@example.com",
        Password = "Password1!",
        PasswordConfirm = "Password1!",
        PhoneNumber = "+12345678901",
        BusinessName = "My Business",
        WebsiteUrl = "https://mybusiness.com"
    };

    // ─── Username ─────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyUsername_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Username = string.Empty;

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Username));
    }

    [Fact]
    public void Validate_WithUsernameLessThan3Chars_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Username = "ab";

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(dto.Username) &&
            e.ErrorMessage.Contains("3 characters"));
    }

    [Fact]
    public void Validate_WithUsernameExactly3Chars_ShouldPass()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Username = "abc";

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeTrue();
    }

    // ─── Email ────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyEmail_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Email = string.Empty;

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    public void Validate_WithInvalidEmailFormat_ShouldFail(string invalidEmail)
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Email = invalidEmail;

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
    }

    // ─── Password ─────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyPassword_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Password = string.Empty;
        dto.PasswordConfirm = string.Empty;

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Password));
    }

    [Fact]
    public void Validate_WithPasswordShorterThan8Chars_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Password = "Ab1!xyz"; // 7 chars
        dto.PasswordConfirm = "Ab1!xyz";

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(dto.Password) &&
            e.ErrorMessage.Contains("8 characters"));
    }

    [Fact]
    public void Validate_WithPasswordMissingUppercase_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Password = "password1!";
        dto.PasswordConfirm = "password1!";

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(dto.Password) &&
            e.ErrorMessage.Contains("uppercase"));
    }

    [Fact]
    public void Validate_WithPasswordMissingLowercase_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Password = "PASSWORD1!";
        dto.PasswordConfirm = "PASSWORD1!";

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(dto.Password) &&
            e.ErrorMessage.Contains("lowercase"));
    }

    [Fact]
    public void Validate_WithPasswordMissingDigit_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Password = "Password!!";
        dto.PasswordConfirm = "Password!!";

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(dto.Password) &&
            e.ErrorMessage.Contains("digit"));
    }

    [Fact]
    public void Validate_WithPasswordMissingSpecialChar_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Password = "Password1A";
        dto.PasswordConfirm = "Password1A";

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(dto.Password) &&
            e.ErrorMessage.Contains("special character"));
    }

    // ─── PasswordConfirm ─────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyPasswordConfirm_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.PasswordConfirm = string.Empty;

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.PasswordConfirm));
    }

    [Fact]
    public void Validate_WithPasswordConfirmNotMatchingPassword_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.PasswordConfirm = "DifferentPass1!";

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(dto.PasswordConfirm) &&
            e.ErrorMessage.Contains("do not match"));
    }

    // ─── PhoneNumber ─────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyPhoneNumber_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.PhoneNumber = string.Empty;

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.PhoneNumber));
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("0123456789")]          // starts with 0 — invalid E.164
    [InlineData("1234567890123456")]    // 16 digits — exceeds max
    public void Validate_WithInvalidPhoneNumber_ShouldFail(string invalidPhone)
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.PhoneNumber = invalidPhone;

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(dto.PhoneNumber) &&
            e.ErrorMessage.Contains("phone number"));
    }

    // ─── BusinessName ────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyBusinessName_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.BusinessName = string.Empty;

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.BusinessName));
    }

    [Fact]
    public void Validate_WithWhitespaceBusinessName_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.BusinessName = "   ";

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.BusinessName));
    }

    [Fact]
    public void Validate_WithBusinessNameLessThan2Chars_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.BusinessName = "A";

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(dto.BusinessName) &&
            e.ErrorMessage.Contains("2 characters"));
    }

    [Fact]
    public void Validate_WithBusinessNameExactly2Chars_ShouldPass()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.BusinessName = "AB";

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidBusinessName_ShouldPass()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.BusinessName = "Acme Corp";

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeTrue();
    }

    // ─── WebsiteUrl ──────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyWebsiteUrl_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.WebsiteUrl = string.Empty;

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.WebsiteUrl));
    }

    [Theory]
    [InlineData("notaurl")]
    [InlineData("www.example.com")]    // relative — not an absolute URI
    [InlineData("ftp:/invalid")]       // malformed
    public void Validate_WithInvalidWebsiteUrl_ShouldFail(string invalidUrl)
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.WebsiteUrl = invalidUrl;

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(dto.WebsiteUrl) &&
            e.ErrorMessage.Contains("URL"));
    }

    [Theory]
    [InlineData("https://mybusiness.com")]
    [InlineData("http://shop.example.org/store")]
    public void Validate_WithValidWebsiteUrl_ShouldPass(string validUrl)
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.WebsiteUrl = validUrl;

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeTrue();
    }

    // ─── Combined ─────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithAllValidData_ShouldPass()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithMultipleInvalidFields_ShouldReturnMultipleErrors()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = new RegisterVendorDTO
        {
            Username = string.Empty,       // fails NotEmpty
            Email = "notanemail",          // fails EmailAddress
            Password = "Password1!",
            PasswordConfirm = "Password1!",
            PhoneNumber = "+12345678901",
            BusinessName = "A",            // fails MinimumLength(2)
            WebsiteUrl = "notaurl"         // fails Must(IsWellFormedUriString)
        };

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Username));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.BusinessName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.WebsiteUrl));
    }
}

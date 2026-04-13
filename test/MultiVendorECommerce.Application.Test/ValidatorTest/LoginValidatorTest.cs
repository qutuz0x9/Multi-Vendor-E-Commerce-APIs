using FluentAssertions;
using MultiVendorECommerce.Application.DTOs.Auth;
using MultiVendorECommerce.Application.Validators.Auth;

namespace MultiVendorECommerce.Application.Test.ValidatorTest;

public class LoginValidatorTest
{
    private readonly LoginValidator _validator;

    public LoginValidatorTest()
    {
        _validator = new LoginValidator();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static LoginRequestDTO ValidDto() => new()
    {
        Email = "user@example.com",
        Password = "Password1!"
    };

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

    [Fact]
    public void Validate_WithValidEmail_ShouldPass()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Email = "user@example.com";

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeTrue();
    }

    // ─── Password ─────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyPassword_ShouldFail()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Password = string.Empty;

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Password));
    }

    [Fact]
    public void Validate_WithNonEmptyPassword_ShouldPass()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = ValidDto();
        dto.Password = "anyvalue";

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
    public void Validate_WithEmptyEmailAndEmptyPassword_ShouldReturnMultipleErrors()
    {
        // ── 1) ARRANGE ────────────────────────
        var dto = new LoginRequestDTO
        {
            Email = string.Empty,
            Password = string.Empty
        };

        // ── 2) ACT ────────────────────────────
        var result = _validator.Validate(dto);

        // ── 3) ASSERT ─────────────────────────
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Password));
    }
}

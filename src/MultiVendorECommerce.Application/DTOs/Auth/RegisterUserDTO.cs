namespace MultiVendorECommerce.Application.DTOs.Auth;

public class RegisterUserDTO
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string PasswordConfirm { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}

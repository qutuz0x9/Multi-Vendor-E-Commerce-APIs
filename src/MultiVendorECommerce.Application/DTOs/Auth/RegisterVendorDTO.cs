

namespace MultiVendorECommerce.Application.DTOs.Auth;

public class RegisterVendorDTO : RegisterUserDTO
{
    public string BusinessName { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
}


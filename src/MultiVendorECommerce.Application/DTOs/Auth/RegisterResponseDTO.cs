namespace MultiVendorECommerce.Application.DTOs.Auth;
public class RegisterResponseDTO
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}


using MultiVendorECommerce.Application.DTOs.Auth;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDTO>> RegisterUser(RegisterUserDTO request);
    Task<Result<AuthResponseDTO>> RegisterVendor(RegisterVendorDTO request);

}

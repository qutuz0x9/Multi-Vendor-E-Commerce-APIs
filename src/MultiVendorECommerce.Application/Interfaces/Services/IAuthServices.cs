using MultiVendorECommerce.Application.DTOs.Auth;
using MultiVendorECommerce.Shared.Results;

namespace MultiVendorECommerce.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<RegisterResponseDTO>> RegisterUser(RegisterUserDTO request);
    //Task<Result<RegisterResponseDTO>> RegisterVendor(RegisterVendorDTO request);

}

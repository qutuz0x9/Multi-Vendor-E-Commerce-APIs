using MultiVendorECommerce.Domain.Models;
using System.Security.Claims;

namespace MultiVendorECommerce.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, IList<string> roles);
    string GenerateRefreshToken();

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

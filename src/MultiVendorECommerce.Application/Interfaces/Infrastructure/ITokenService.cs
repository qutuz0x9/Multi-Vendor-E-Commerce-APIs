using MultiVendorECommerce.Domain.Models;
using System.Security.Claims;

namespace MultiVendorECommerce.Application.Interfaces.Infrastructure;

public interface ITokenService

{
    string GenerateAccessToken(User user, IList<string> roles, Guid? cartSessionId = null);
    string GenerateRefreshToken();

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

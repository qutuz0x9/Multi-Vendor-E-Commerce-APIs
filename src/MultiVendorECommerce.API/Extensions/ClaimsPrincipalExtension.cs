using System.Security.Claims;
namespace MultiVendorECommerce.API.Extensions;
public static class ClaimsPrincipalExtension
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            throw new Exception("User ID claim not found");
        }
        return Guid.Parse(userIdClaim);
    }
}
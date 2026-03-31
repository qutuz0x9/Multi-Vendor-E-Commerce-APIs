using Microsoft.AspNetCore.Identity;

namespace MultiVendorECommerce.Domain.Models;

public class RoleClaim : IdentityRoleClaim<Guid>
{
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

using Microsoft.AspNetCore.Identity;

namespace MultiVendorECommerce.Domain.Models;

public class UserRole : IdentityUserRole<Guid>
{
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

using Microsoft.AspNetCore.Identity;

namespace MultiVendorECommerce.Domain.Models;

public class UserLogin : IdentityUserLogin<Guid>
{
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

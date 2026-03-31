using Microsoft.AspNetCore.Identity;
using MultiVendorECommerce.Domain.Enums;

namespace MultiVendorECommerce.Domain.Models;

public class User : IdentityUser<Guid>
{
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

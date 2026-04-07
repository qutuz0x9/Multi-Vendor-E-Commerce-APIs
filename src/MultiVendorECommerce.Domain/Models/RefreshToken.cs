using System.ComponentModel.DataAnnotations;

namespace MultiVendorECommerce.Domain.Models;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;           // Long random string
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }                // Token rotation support
    public bool IsRevoked { get; set; } = false;
    public bool IsUsed { get; set; } = false;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;

}

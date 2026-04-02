using MultiVendorECommerce.Domain.Enums;
using Npgsql.Replication;

namespace MultiVendorECommerce.Domain.Models;

public class Vendor
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = null!;
    public string WebsiteUrl { get; set; } = null!;
    public VendorStatus Status { get; set; }
    public decimal? AverageRate { get; set; }
    public int? TotalReviews { get; set; }
    public string? Slug { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public User User { get; set; } = null!;

}



using NpgsqlTypes;

namespace MultiVendorECommerce.Domain.Enums;

public enum VendorStatus
{
    [PgName("pending")]
    Pending,
    [PgName("approved")]
    Approved,
    [PgName("rejected")]
    Rejected,
    [PgName("suspended")]
    Suspended
}

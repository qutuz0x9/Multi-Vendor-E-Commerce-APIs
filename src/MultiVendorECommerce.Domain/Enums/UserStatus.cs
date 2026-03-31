
using NpgsqlTypes;

namespace MultiVendorECommerce.Domain.Enums;

public enum UserStatus
{
    [PgName("active")]
    Active,
    [PgName("suspended")]
    Suspended,
    [PgName("banned")]
    Banned
}

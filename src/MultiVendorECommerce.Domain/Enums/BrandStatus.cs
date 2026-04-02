using NpgsqlTypes;

namespace MultiVendorECommerce.Domain.Enums;

public enum BrandStatus
{
    [PgName("active")]
    Active,
    [PgName("inactive")]
    Inactive,
}

using NpgsqlTypes;

namespace MultiVendorECommerce.Domain.Enums;

public enum ProductStatus
{
    [PgName("active")]
    Active,
    [PgName("inactive")]
    Inactive,
    [PgName("drafted")]
    Drafted,
}

using NpgsqlTypes;

namespace MultiVendorECommerce.Domain.Enums;

public enum InventoryStatus
{
    [PgName("available")]
    Available,
    [PgName("depleted")]
    Depleted,
}

using NpgsqlTypes;

namespace MultiVendorECommerce.Domain.Enums;

public enum VendorAddressType
{
    [PgName("warehouse")]
    Warehouse,
    [PgName("pickup_point")]
    PickupPoint,
    [PgName("return")]
    Return
}

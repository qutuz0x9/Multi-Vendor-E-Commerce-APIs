using NpgsqlTypes;

namespace MultiVendorECommerce.Domain.Enums;

public enum CustomerAddressType
{
    [PgName("shipping")]
    Shipping,
    [PgName("billing")]
    Billing,
    [PgName("pickup")]
    Pickup,
}

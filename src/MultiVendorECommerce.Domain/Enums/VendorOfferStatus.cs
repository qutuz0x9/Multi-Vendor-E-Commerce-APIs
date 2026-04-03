using NpgsqlTypes;

namespace MultiVendorECommerce.Domain.Enums;

public enum VendorOfferStatus
{
    [PgName("active")]
    Active,
    [PgName("out_of_stock")]
    OutOfStock,
    [PgName("inactive")]
    Inactive,
}

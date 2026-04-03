using NpgsqlTypes;

namespace MultiVendorECommerce.Domain.Enums;

public enum OrderStatus
{
    [PgName("pending")]
    Pending,
    [PgName("confirmed")]
    Confirmed,
    [PgName("shipped")]
    Shipped,
    [PgName("delivered")]
    Delivered,
    [PgName("cancelled")]
    Cancelled,
}

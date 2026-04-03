using NpgsqlTypes;

namespace MultiVendorECommerce.Domain.Enums;

public enum PaymentStatus
{
    [PgName("pending")]
    Pending,
    [PgName("completed")]
    Completed,
    [PgName("failed")]
    Failed,
    [PgName("refunded")]
    Refunded,
}

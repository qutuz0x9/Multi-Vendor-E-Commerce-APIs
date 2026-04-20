using MultiVendorECommerce.Application.Interfaces.Repositories;

namespace MultiVendorECommerce.Application.Interfaces.Infrastructure;

public interface IUnitOfWork : IAsyncDisposable
{
    IProductRepository Products { get; }
    IBrandRepository Brands { get; }
    ICategoryRepository Categories { get; }
    IVendorRepository Vendors { get; }
    ICustomerRepository Customers { get; }
    IVendorOfferRepository VendorOffers { get; }
    IOrderRepository Orders { get; }
    IOrderItemRepository OrderItems { get; }
    IPaymentRepository Payments { get; }
    IInventoryRepository Inventories { get; }
    ICustomerAddressRepository CustomerAddresses { get; }
    IVendorAddressRepository VendorAddresses { get; }
    IProductCategoryRepository ProductCategories { get; }
    ICartSessionRepository CartSessions { get; }
    ICartItemRepository CartItems { get; }
    IOrderShippingAddressRepository OrderShippingAddresses { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    Task<int> SaveChangesAsync();
    /// <summary>Returns true on success, false when a unique-constraint violation is detected.</summary>
    Task<bool> TrySaveChangesAsync();
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitTransactionAsync();
    Task<bool> RollbackTransactionAsync();
}

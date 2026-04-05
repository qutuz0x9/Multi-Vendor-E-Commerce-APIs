using MultiVendorECommerce.Application.Interfaces.Repositories;

namespace MultiVendorECommerce.Application.Interfaces;

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

    Task<int> SaveChangesAsync();
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitTransactionAsync();
    Task<bool> RollbackTransactionAsync();
}

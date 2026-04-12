using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Infrastructure.Contexts;
using MultiVendorECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace MultiVendorECommerce.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ECommerceDbContext _context;
    private IDbContextTransaction? _transaction;

    private IProductRepository? _productRepository;
    private IBrandRepository? _brandRepository;
    private ICategoryRepository? _categoryRepository;
    private IVendorRepository? _vendorRepository;
    private ICustomerRepository? _customerRepository;
    private IVendorOfferRepository? _vendorOfferRepository;
    private IOrderRepository? _orderRepository;
    private IOrderItemRepository? _orderItemRepository;
    private IPaymentRepository? _paymentRepository;
    private IInventoryRepository? _inventoryRepository;
    private ICustomerAddressRepository? _customerAddressRepository;
    private IVendorAddressRepository? _vendorAddressRepository;
    private IProductCategoryRepository? _productCategoryRepository;
    private ICartSessionRepository? _cartSessionRepository;
    private ICartItemRepository? _cartItemRepository;
    private IOrderShippingAddressRepository? _orderShippingAddressRepository;

    public UnitOfWork(ECommerceDbContext context)
    {
        _context = context;
    }

    public IProductRepository Products =>
        _productRepository ??= new ProductRepository(_context);

    public IBrandRepository Brands =>
        _brandRepository ??= new BrandRepository(_context);

    public ICategoryRepository Categories =>
        _categoryRepository ??= new CategoryRepository(_context);

    public IVendorRepository Vendors =>
        _vendorRepository ??= new VendorRepository(_context);

    public ICustomerRepository Customers =>
        _customerRepository ??= new CustomerRepository(_context);

    public IVendorOfferRepository VendorOffers =>
        _vendorOfferRepository ??= new VendorOfferRepository(_context);

    public IOrderRepository Orders =>
        _orderRepository ??= new OrderRepository(_context);

    public IOrderItemRepository OrderItems =>
        _orderItemRepository ??= new OrderItemRepository(_context);

    public IPaymentRepository Payments =>
        _paymentRepository ??= new PaymentRepository(_context);

    public IInventoryRepository Inventories =>
        _inventoryRepository ??= new InventoryRepository(_context);

    public ICustomerAddressRepository CustomerAddresses =>
        _customerAddressRepository ??= new CustomerAddressRepository(_context);

    public IVendorAddressRepository VendorAddresses =>
        _vendorAddressRepository ??= new VendorAddressRepository(_context);

    public IProductCategoryRepository ProductCategories =>
        _productCategoryRepository ??= new ProductCategoryRepository(_context);

    public ICartSessionRepository CartSessions =>
        _cartSessionRepository ??= new CartSessionRepository(_context);

    public ICartItemRepository CartItems =>
        _cartItemRepository ??= new CartItemRepository(_context);

    public IOrderShippingAddressRepository OrderShippingAddresses =>
        _orderShippingAddressRepository ??= new OrderShippingAddressRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<bool> BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        return _transaction != null;
    }

    public async Task<bool> CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
            return true;
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task<bool> RollbackTransactionAsync()
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
            return true;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}

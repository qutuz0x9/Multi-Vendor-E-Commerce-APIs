using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MultiVendorECommerce.Application.DTOs.Auth;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Domain.Models;
using AutoMapper;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Shared.Constants;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Shared.Enums;

namespace MultiVendorECommerce.Application.Test.AuthServiceTest;

public class RegisterNewVendorTest
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVendorRepository> _vendorRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ICookieService> _cookieServiceMock;

    public RegisterNewVendorTest()
    {
        var userStoreMock = Mock.Of<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock,                  // 1. IUserStore<User>
            null!,                           // 2. IOptions<IdentityOptions>
            null!,                           // 3. IPasswordHasher<User>
            null!,                           // 4. IEnumerable<IUserValidator<User>>
            null!,                           // 5. IEnumerable<IPasswordValidator<User>>
            null!,                           // 6. ILookupNormalizer
            null!,                           // 7. IdentityErrorDescriber
            null!,                           // 8. IServiceProvider
            null!                            // 9. ILogger<UserManager<User>>
        );
        var roleStore = Mock.Of<IRoleStore<Role>>();
        _roleManagerMock = new Mock<RoleManager<Role>>(
            roleStore,                  // IRoleStore<Role>
            null!,                       // IEnumerable<IRoleValidator<Role>>
            null!,                       // ILookupNormalizer
            null!,                       // IdentityErrorDescriber
            null!                        // ILogger<RoleManager<Role>>
        );
        _mapper = MapperTestHelper.GetMapper();
        _tokenServiceMock = new Mock<ITokenService>();
        _cookieServiceMock = new Mock<ICookieService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _vendorRepositoryMock = new Mock<IVendorRepository>();

        // Setup transaction methods on UnitOfWork
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.Vendors).Returns(_vendorRepositoryMock.Object);
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepositoryMock.Object);
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("dummy_refresh_token");

        _authService = new AuthService(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _unitOfWorkMock.Object,
            _mapper,
            _tokenServiceMock.Object,
            _cookieServiceMock.Object
        );
    }

    [Fact]
    public async Task RegisterNewVendor_WithValidData_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "testvendor",
            Email = "vendor@gmail.com",
            Password = "test@123",
            PasswordConfirm = "test@123",
            BusinessName = "Test Business",
            WebsiteUrl = "https://testvendor.com"
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(u => u.FindByNameAsync(request.Username))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync(Roles.Vendor))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor))
            .ReturnsAsync(IdentityResult.Success);
        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>()))
            .Returns("dummy_token");
        _userManagerMock
            .Setup(u => u.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { Roles.Vendor });

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        _userManagerMock.Verify(u => u.FindByNameAsync(request.Username), Times.Once);
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
        _roleManagerMock.Verify(r => r.RoleExistsAsync(Roles.Vendor), Times.Once);
        _userManagerMock.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor), Times.Once);
        _tokenServiceMock.Verify(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>()), Times.Once);

        // Verify transaction was committed and never rolled back
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);

        // Verify vendor profile was created
        _vendorRepositoryMock.Verify(v => v.AddAsync(It.Is<Vendor>(
            vendor => vendor.UserId != Guid.Empty && vendor.BusinessName == request.Username
        )), Times.Once);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().NotBeEmpty();
        result.Value.UserName.Should().Be(request.Username);
        result.Value.Role.Should().Be(Roles.Vendor);
        result.Value.Token.Should().Be("dummy_token");
    }

    [Fact]
    public async Task RegisterNewVendor_WithDuplicateEmail_ShouldReturnFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "testvendor",
            Email = "vendor@gmail.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };
        var existingUser = new User { Id = Guid.NewGuid(), Email = request.Email, UserName = "anothervendor" };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);

        // Verify transaction was rolled back and never committed
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task RegisterNewVendor_WithDuplicateUsername_ShouldReturnFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "testvendor",
            Email = "vendor@gmail.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };
        var existingUser = new User { Id = Guid.NewGuid(), Email = "another@gmail.com", UserName = request.Username };

        _userManagerMock
            .Setup(u => u.FindByNameAsync(request.Username))
            .ReturnsAsync(existingUser);

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _userManagerMock.Verify(u => u.FindByNameAsync(request.Username), Times.Once);
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);

        // Verify transaction was rolled back and never committed
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task RegisterNewVendor_WhenUserCreationFails_ShouldReturnFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "testvendor",
            Email = "vendor@email.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User creation failed." }));

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);

        // Verify transaction was rolled back and never committed
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Failure);
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task RegisterNewVendor_WhenCreationReturnsMultipleErrors_ShouldAggregateErrors()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "testvendor",
            Email = "vendor@email.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Error 1" },
                new IdentityError { Description = "Error 2" }
            ));

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors[0].Type.Should().Be(ErrorType.Failure);
        result.Errors[0].ErrorMessage.Should().Be("Error 1");
        result.Errors[1].Type.Should().Be(ErrorType.Failure);
        result.Errors[1].ErrorMessage.Should().Be("Error 2");
        result.StatusCode.Should().Be(500);

        // Verify transaction was rolled back and never committed
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task RegisterNewVendor_WhenVendorRoleDoesNotExist_ShouldCreateRole()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "testvendor",
            Email = "vendor@email.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        // Simulate the Vendor role not existing
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync(Roles.Vendor))
            .ReturnsAsync(false);
        _roleManagerMock
            .Setup(r => r.CreateAsync(It.Is<Role>(role => role.Name == Roles.Vendor)))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor))
            .ReturnsAsync(IdentityResult.Success);

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _roleManagerMock.Verify(r => r.RoleExistsAsync(Roles.Vendor), Times.Once);
        _roleManagerMock.Verify(r => r.CreateAsync(It.Is<Role>(role => role.Name == Roles.Vendor)), Times.Once);
        _userManagerMock.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor), Times.Once);

        // Verify transaction was committed
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterNewVendor_WhenVendorRoleAlreadyExists_ShouldNotCreateRole()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "testvendor",
            Email = "vendor@email.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync(Roles.Vendor))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor))
            .ReturnsAsync(IdentityResult.Success);

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _roleManagerMock.Verify(r => r.RoleExistsAsync(Roles.Vendor), Times.Once);
        _roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<Role>()), Times.Never);
        _userManagerMock.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor), Times.Once);

        // Verify transaction was committed
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterNewVendor_WhenRoleAssignmentFails_ShouldReturn500Failure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "testvendor",
            Email = "vendor@email.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync(Roles.Vendor))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed." }));

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
        _roleManagerMock.Verify(r => r.RoleExistsAsync(Roles.Vendor), Times.Once);
        _userManagerMock.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor), Times.Once);

        // Verify transaction was rolled back and never committed
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Failure);
        result.Errors[0].ErrorMessage.Should().Be("Failed to assign role to user.");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task RegisterNewVendor_WhenRoleAssignmentFails_ShouldNotGenerateToken()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "testvendor",
            Email = "vendor@email.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync(Roles.Vendor))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed." }));
        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>()))
            .Returns("dummy_token");

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
        _roleManagerMock.Verify(r => r.RoleExistsAsync(Roles.Vendor), Times.Once);
        _userManagerMock.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor), Times.Once);
        _tokenServiceMock.Verify(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>()), Times.Never);

        // Verify transaction was rolled back and never committed
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Failure);
        result.Errors[0].ErrorMessage.Should().Be("Failed to assign role to user.");
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task RegisterNewVendor_WithValidData_ShouldCallGenerateAccessTokenWithCorrectRoles()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "testvendor",
            Email = "vendor@email.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync(Roles.Vendor))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(u => u.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { Roles.Vendor });
        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>()))
            .Returns("dummy_token");

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
        _roleManagerMock.Verify(r => r.RoleExistsAsync(Roles.Vendor), Times.Once);
        _userManagerMock.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor), Times.Once);
        _tokenServiceMock.Verify(t => t.GenerateAccessToken(It.IsAny<User>(), It.Is<IList<string>>(roles => roles.Contains(Roles.Vendor))), Times.Once);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterNewVendor_WithValidData_ShouldCreateVendorProfile()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "testvendor",
            Email = "vendor@gmail.com",
            Password = "test@123",
            PasswordConfirm = "test@123",
            BusinessName = "Test Business",
            WebsiteUrl = "https://testvendor.com"
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(u => u.FindByNameAsync(request.Username))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync(Roles.Vendor))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(u => u.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { Roles.Vendor });
        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>()))
            .Returns("dummy_token");

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _vendorRepositoryMock.Verify(v => v.AddAsync(It.Is<Vendor>(
            vendor => vendor.UserId != Guid.Empty
                && vendor.BusinessName == request.Username
                && vendor.WebsiteUrl == $"https://{request.Username.ToLower()}.com"
                && vendor.Slug != null
        )), Times.Once);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterNewVendor_WithValidData_ShouldGenerateSlugFromUsername()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "Test Vendor Name",
            Email = "vendor@gmail.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(u => u.FindByNameAsync(request.Username))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync(Roles.Vendor))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(u => u.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { Roles.Vendor });
        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>()))
            .Returns("dummy_token");

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _vendorRepositoryMock.Verify(v => v.AddAsync(It.Is<Vendor>(
            vendor => vendor.Slug == "test-vendor-name"
        )), Times.Once);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterNewVendor_WhenRoleAssignmentFails_ShouldNotCreateVendorProfile()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "testvendor",
            Email = "vendor@email.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync(Roles.Vendor))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Vendor))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed." }));

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _vendorRepositoryMock.Verify(v => v.AddAsync(It.IsAny<Vendor>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);

        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task RegisterNewVendor_WhenDuplicateEmail_ShouldNotCreateVendorProfile()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterVendorDTO
        {
            Username = "testvendor",
            Email = "vendor@gmail.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };
        var existingUser = new User { Id = Guid.NewGuid(), Email = request.Email, UserName = "anothervendor" };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterVendor(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _vendorRepositoryMock.Verify(v => v.AddAsync(It.IsAny<Vendor>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);

        result.IsFailure.Should().BeTrue();
    }
}

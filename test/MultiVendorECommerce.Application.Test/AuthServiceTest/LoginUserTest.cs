using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MultiVendorECommerce.Application.DTOs.Auth;
using MultiVendorECommerce.Application.Interfaces.Infrastructure;
using MultiVendorECommerce.Application.Interfaces.Repositories;
using MultiVendorECommerce.Application.Interfaces.Services;
using MultiVendorECommerce.Application.Services;
using MultiVendorECommerce.Application.Test.Helpers;
using MultiVendorECommerce.Domain.Models;
using MultiVendorECommerce.Shared.Constants;
using MultiVendorECommerce.Shared.Enums;
using MultiVendorECommerce.Shared.Logging;

namespace MultiVendorECommerce.Application.Test.AuthServiceTest;

public class LoginUserTest
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ICartSessionRepository> _cartSessionRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ICookieService> _cookieServiceMock;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public LoginUserTest()
    {
        var userStoreMock = Mock.Of<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );

        var roleStoreMock = Mock.Of<IRoleStore<Role>>();
        _roleManagerMock = new Mock<RoleManager<Role>>(
            roleStoreMock,
            null!,
            null!,
            null!,
            null!
        );

        _mapper = MapperTestHelper.GetMapper();
        _tokenServiceMock = new Mock<ITokenService>();
        _cookieServiceMock = new Mock<ICookieService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepositoryMock.Object);
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _cartSessionRepositoryMock = new Mock<ICartSessionRepository>();
        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CartSessions).Returns(_cartSessionRepositoryMock.Object);
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("dummy_refresh_token");
        var loggerMock = new Mock<IAppLogger<AuthService>>();

        _authService = new AuthService(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _unitOfWorkMock.Object,
            _mapper,
            _tokenServiceMock.Object,
            _cookieServiceMock.Object,
            loggerMock.Object
        );
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new LoginRequestDTO
        {
            Email = "test@gmail.com",
            Password = "test@123"
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = request.Email
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        _userManagerMock
            .Setup(u => u.CheckPasswordAsync(existingUser, request.Password))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(u => u.GetRolesAsync(existingUser))
            .ReturnsAsync(new List<string> { Roles.Customer });

        var customer = new Customer { Id = Guid.NewGuid(), UserId = existingUser.Id };
        var cartSession = new CartSession { Id = Guid.NewGuid(), CustomerId = customer.Id };

        _customerRepositoryMock
            .Setup(c => c.GetCustomerByUserIdAsync(existingUser.Id))
            .ReturnsAsync(customer);
        _cartSessionRepositoryMock
            .Setup(c => c.GetCartByCustomerAsync(customer.Id))
            .ReturnsAsync(cartSession);

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(existingUser, It.IsAny<IList<string>>(), It.IsAny<Guid?>()))
            .Returns("dummy_token");

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _authService.Login(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(existingUser.Id);
        result.Value.UserName.Should().Be(existingUser.UserName);
        result.Value.Role.Should().Be(Roles.Customer);
        result.Value.Token.Should().Be("dummy_token");

        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        _userManagerMock.Verify(u => u.CheckPasswordAsync(existingUser, request.Password), Times.Once);
        _userManagerMock.Verify(u => u.GetRolesAsync(existingUser), Times.Once);
        _customerRepositoryMock.Verify(c => c.GetCustomerByUserIdAsync(existingUser.Id), Times.Once);
        _cartSessionRepositoryMock.Verify(c => c.GetCartByCustomerAsync(customer.Id), Times.Once);
        _tokenServiceMock.Verify(t => t.GenerateAccessToken(existingUser, It.IsAny<IList<string>>(), It.Is<Guid?>(id => id.HasValue && id.Value == cartSession.Id)), Times.Once);
        _tokenServiceMock.Verify(t => t.GenerateRefreshToken(), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        _cookieServiceMock.Verify(c => c.SetCookie("refreshToken", It.IsAny<string>(), 7), Times.Once);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldReturnValidationFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new LoginRequestDTO
        {
            Email = "notfound@gmail.com",
            Password = "test@123"
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _authService.Login(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.Errors[0].ErrorMessage.Should().Be("Invalid email or password.");

        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        _userManagerMock.Verify(u => u.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        _tokenServiceMock.Verify(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>(), It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnValidationFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new LoginRequestDTO
        {
            Email = "test@gmail.com",
            Password = "wrongpassword"
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = request.Email
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        _userManagerMock
            .Setup(u => u.CheckPasswordAsync(existingUser, request.Password))
            .ReturnsAsync(false);

        // ── 2) ACT ────────────────────────────────────────────────────────────────
        var result = await _authService.Login(request);

        // ── 3) ASSERT ─────────────────────────────────────────────────────────────
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
        result.Errors[0].ErrorMessage.Should().Be("Invalid email or password.");

        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        _userManagerMock.Verify(u => u.CheckPasswordAsync(existingUser, request.Password), Times.Once);
        _tokenServiceMock.Verify(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>(), It.IsAny<Guid?>()), Times.Never);
        _tokenServiceMock.Verify(t => t.GenerateRefreshToken(), Times.Never);
    }
}
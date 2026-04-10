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

namespace MultiVendorECommerce.Application.Test.AuthServiceTest;

public class RegisterNewUserTest
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public RegisterNewUserTest()
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
        _authService = new AuthService(_userManagerMock.Object, _roleManagerMock.Object, _mapper);
    }


    /// <summary>
    /// Impement Happy path test for RegisterNewUser method in AuthService.
    /// This test should verify that when valid data is provided,
    /// the method returns a success result with the expected AuthResponseDTO.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RegisterNewUser_WithValidData_ShouldReturnSuccess()
    {

        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        // Create Request Object (Here I have the Request, So the next step is to check for the data
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "test@gmail",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        // Setup Mocks (Here is the Setup of Mocks 'UserManager' to Check the Inputs
        // The Actual Return Should Be Null, So that it's prove there is no User with this email
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        // The Actual Return Should Be Null, So taht it's prove there is no user with this username
        _userManagerMock
            .Setup(u => u.FindByNameAsync(request.Username))
            .ReturnsAsync((User?)null);
        // Setup the CreateAsync method to return Success when called with any User and the specified password
        _userManagerMock
        .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
        .ReturnsAsync(IdentityResult.Success);
        // Setup the RoleExistsAsync method to return true when called with the specified role
        _roleManagerMock
        .Setup(r => r.RoleExistsAsync(Roles.Customer))
        .ReturnsAsync(true);
        // Setup the AddToRoleAsync method to return Success when called with any User and the specified role
        _userManagerMock
        .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer))
        .ReturnsAsync(IdentityResult.Success);
        
        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterUser(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserName.Should().Be(request.Username);
    }
}

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

public class RegisterNewUserTest
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ICookieService> _cookieServiceMock;


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
        _tokenServiceMock = new Mock<ITokenService>();
        _cookieServiceMock = new Mock<ICookieService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();

        // Setup transaction methods on UnitOfWork
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepositoryMock.Object);

        _authService = new AuthService(
        _userManagerMock.Object,
        _roleManagerMock.Object,
        _unitOfWorkMock.Object,
        _mapper,
        _tokenServiceMock.Object,
        _cookieServiceMock.Object
        );
    }


    /// <summary>
    /// Impement Happy path test for RegisterNewUser method in AuthService.
    /// This test should verify that when valid data is provided,
    /// the method returns a success result with the expected AuthResponseDTO.
    /// Steps to Register New User:
    /// 1) Check If There is User with this Email or Username (Should be Null)
    /// 2) Create User with the Request Data
    /// 3) Assign Customer Role For this User
    /// 4) Generate Token For This User
    /// 5) Return Result with the Expected Response DTO
    /// </summary>
    [Fact]
    public async Task RegisterNewUser_WithValidData_ShouldReturnSuccess()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        // Create Request Object (Here I have the Request, So the next step is to check for the data
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "test@gmail.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        // Setup Mocks (Here is the Setup of Mocks 'UserManager' to Check the Inputs
        // The Actual Return Should Be Null, So that it's prove there is no User with this email
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
        // The Actual Return Should Be Null, So that it's prove there is no user with this username
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
        // Setup the GenerateAccessToken method to return a dummy token when called with any User and list of roles
        _tokenServiceMock
        .Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>()))
        .Returns("dummy_token");
        // Setup GetRolesAsync to return the Customer role after assignment
        _userManagerMock
        .Setup(u => u.GetRolesAsync(It.IsAny<User>()))
        .ReturnsAsync(new List<string> { Roles.Customer });


        // ── 2) ACT ────────────────────────────────────────────────────────────
        // Call the RegisterUser method with the request object and capture the result
        var result = await _authService.RegisterUser(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        // Verify that the UserManager's FindByEmailAsync method was called once with the correct email
        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        // Verify that the UserManager's FindByNameAsync method was called once with the correct username
        _userManagerMock.Verify(u => u.FindByNameAsync(request.Username), Times.Once);
        // Verify that the UserManager's CreateAsync method was called once with any User and the correct password
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
        // Verify that the RoleManager's RoleExistsAsync method was called once with the correct role
        _roleManagerMock.Verify(r => r.RoleExistsAsync(Roles.Customer), Times.Once);
        // Verify that the UserManager's AddToRoleAsync method was called once with any User and the correct role
        _userManagerMock.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer), Times.Once);
        // Verify that the TokenService's GenerateAccessToken method was called once with any User and any list of roles
        _tokenServiceMock.Verify(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>()), Times.Once);

        // Verify transaction was committed and never rolled back
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);

        // Verify customer profile was created
        _customerRepositoryMock.Verify(c => c.AddAsync(It.Is<Customer>(
            cust => cust.UserId != Guid.Empty && cust.FirstName == request.Username
        )), Times.Once);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().NotBeEmpty();
        result.Value.UserName.Should().Be(request.Username);
        result.Value.Role.Should().Be(Roles.Customer);
        result.Value.Token.Should().Be("dummy_token");
    }

    [Fact]
    public async Task RegisterNewUser_WithDuplicateEmail_ShouldReturnFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        // Create Request Object
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "test@gmail.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };
        // Create an existing user with the same email to simulate a duplicate email scenario
        var existingUser = new User { Id = Guid.NewGuid(), Email = request.Email, UserName = "anotheruser" };

        // Setup FindByEmailAsync to return an existing user (simulating a duplicate)
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterUser(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        // Verify FindByEmailAsync was called and CreateAsync was never reached
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
    public async Task RegisterNewUser_WithDuplicateUsername_ShouldReturnFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        // Create Request Object
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "test@gmail.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };
        // Create an existing user with the same username to simulate a duplicate username scenario
        var existingUser = new User { Id = Guid.NewGuid(), Email = "another@gmail.com", UserName = request.Username };

        // Setup FindByNameAsync to return an existing user (simulating a duplicate username)
        _userManagerMock
        .Setup(u => u.FindByNameAsync(request.Username))
        .ReturnsAsync(existingUser);

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterUser(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        // Verify FindByNameAsync was called and CreateAsync was never reached
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
    public async Task RegisterNewUser_WhenUserCreationFails_ShouldReturnFailure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "testuser@email",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        _userManagerMock
        .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User creation failed." }));

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterUser(request);

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
    public async Task RegisterNewUser_WhenCreationReturnsMultipleErrors_ShouldAggregateErrors()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "testuser@email",
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
        var result = await _authService.RegisterUser(request);

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
    public async Task RegisterNewUser_WhenCustomerRoleDoesNotExist_ShouldCreateRole()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "testuser@email",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        // Setup CreateAsync to return Success when called with any User and the specified password
        _userManagerMock
        .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
        .ReturnsAsync(IdentityResult.Success);
        // Setup RoleExistsAsync to return false to simulate the role not existing
        _roleManagerMock
        .Setup(r => r.RoleExistsAsync(Roles.Customer))
        .ReturnsAsync(false);
        // Setup CreateAsync for RoleManager to return Success when called with a Role that has the correct name
        _roleManagerMock
        .Setup(r => r.CreateAsync(It.Is<Role>(role => role.Name == Roles.Customer)))
        .ReturnsAsync(IdentityResult.Success);
        // Setup AddToRoleAsync to return Success when called with any User and the specified role
        _userManagerMock
        .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer))
        .ReturnsAsync(IdentityResult.Success);


        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterUser(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _roleManagerMock.Verify(r => r.RoleExistsAsync(Roles.Customer), Times.Once);
        _roleManagerMock.Verify(r => r.CreateAsync(It.Is<Role>(role => role.Name == Roles.Customer)), Times.Once);
        _userManagerMock.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer), Times.Once);

        // Verify transaction was committed
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterNewUser_WhenCustomerRoleAlreadyExists_ShouldNotCreateRole()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "testuser@email",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        // Setup CreateAsync to return Success when called with any User and the specified password
        _userManagerMock
        .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
        .ReturnsAsync(IdentityResult.Success);
        // Setup RoleExistsAsync to return true to simulate the role already existing
        _roleManagerMock
        .Setup(r => r.RoleExistsAsync(Roles.Customer))
        .ReturnsAsync(true);
        // Setup AddToRoleAsync to return Success when called with any User and the specified role
        _userManagerMock
        .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer))
        .ReturnsAsync(IdentityResult.Success);

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterUser(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _roleManagerMock.Verify(r => r.RoleExistsAsync(Roles.Customer), Times.Once);
        _roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<Role>()), Times.Never);
        _userManagerMock.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer), Times.Once);

        // Verify transaction was committed
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterNewUser_WhenRoleAssignmentFails_ShouldReturn500Failure()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "testuser@email",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        // Setup CreateAsync to return Success when called with any User and the specified password
        _userManagerMock
        .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
        .ReturnsAsync(IdentityResult.Success);
        // Setup RoleExistsAsync to return true to simulate the role existing
        _roleManagerMock
        .Setup(r => r.RoleExistsAsync(Roles.Customer))
        .ReturnsAsync(true);
        // Setup AddToRoleAsync to return Failed to simulate a role assignment failure
        _userManagerMock
        .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer))
        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed." }));

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterUser(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
        _roleManagerMock.Verify(r => r.RoleExistsAsync(Roles.Customer), Times.Once);
        _userManagerMock.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer), Times.Once);

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
    public async Task RegisterNewUser_WhenRoleAssignmentFails_ShouldNotGenerateToken()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "testuser@email",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        // Setup CreateAsync to return Success when called with any User and the specified password
        _userManagerMock
        .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
        .ReturnsAsync(IdentityResult.Success);
        // Setup RoleExistsAsync to return true to simulate the role existing
        _roleManagerMock
        .Setup(r => r.RoleExistsAsync(Roles.Customer))
        .ReturnsAsync(true);
        // Setup AddToRoleAsync to return Failed to simulate a role assignment failure
        _userManagerMock
        .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer))
        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed." }));

        _tokenServiceMock
        .Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>()))
        .Returns("dummy_token");

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterUser(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
        _roleManagerMock.Verify(r => r.RoleExistsAsync(Roles.Customer), Times.Once);
        _userManagerMock.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer), Times.Once);
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
    public async Task RegisterNewUser_WithValidData_ShouldCallGenerateAccessTokenWithCorrectRoles()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "testuser@email",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        _userManagerMock
        .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
        .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock
        .Setup(r => r.RoleExistsAsync(Roles.Customer))
        .ReturnsAsync(true);
        _userManagerMock
        .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer))
        .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
        .Setup(u => u.GetRolesAsync(It.IsAny<User>()))
        .ReturnsAsync(new List<string> { Roles.Customer });
        _tokenServiceMock
        .Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>()))
        .Returns("dummy_token");

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterUser(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<User>(), request.Password), Times.Once);
        _roleManagerMock.Verify(r => r.RoleExistsAsync(Roles.Customer), Times.Once);
        _userManagerMock.Verify(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer), Times.Once);
        _tokenServiceMock.Verify(t => t.GenerateAccessToken(It.IsAny<User>(), It.Is<IList<string>>(roles => roles.Contains(Roles.Customer))), Times.Once);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterNewUser_WithValidData_ShouldCreateCustomerProfile()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "test@gmail.com",
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
        .Setup(r => r.RoleExistsAsync(Roles.Customer))
        .ReturnsAsync(true);
        _userManagerMock
        .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer))
        .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
        .Setup(u => u.GetRolesAsync(It.IsAny<User>()))
        .ReturnsAsync(new List<string> { Roles.Customer });
        _tokenServiceMock
        .Setup(t => t.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IList<string>>()))
        .Returns("dummy_token");

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterUser(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _customerRepositoryMock.Verify(c => c.AddAsync(It.Is<Customer>(
            cust => cust.UserId != Guid.Empty
                && cust.FirstName == request.Username
                && cust.LastName == request.Username
        )), Times.Once);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterNewUser_WhenRoleAssignmentFails_ShouldNotCreateCustomerProfile()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "testuser@email",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };

        _userManagerMock
        .Setup(u => u.CreateAsync(It.IsAny<User>(), request.Password))
        .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock
        .Setup(r => r.RoleExistsAsync(Roles.Customer))
        .ReturnsAsync(true);
        _userManagerMock
        .Setup(r => r.AddToRoleAsync(It.IsAny<User>(), Roles.Customer))
        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed." }));

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterUser(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _customerRepositoryMock.Verify(c => c.AddAsync(It.IsAny<Customer>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);

        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task RegisterNewUser_WhenDuplicateEmail_ShouldNotCreateCustomerProfile()
    {
        // ── 1) ARRANGE ────────────────────────────────────────────────────────────
        var request = new RegisterUserDTO
        {
            Username = "testuser",
            Email = "test@gmail.com",
            Password = "test@123",
            PasswordConfirm = "test@123"
        };
        var existingUser = new User { Id = Guid.NewGuid(), Email = request.Email, UserName = "anotheruser" };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // ── 2) ACT ────────────────────────────────────────────────────────────
        var result = await _authService.RegisterUser(request);

        // ── 3) ASSERT ────────────────────────────────────────────────────────────
        _customerRepositoryMock.Verify(c => c.AddAsync(It.IsAny<Customer>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);

        result.IsFailure.Should().BeTrue();
    }
}

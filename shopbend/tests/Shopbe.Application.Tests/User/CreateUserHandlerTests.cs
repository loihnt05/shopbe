using AutoMapper;
using Moq;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.IUser;
using Shopbe.Application.User.Users.Commands.CreateUser;
using Shopbe.Application.User.Users.Dtos;
using DomainUser = Shopbe.Domain.Entities.User.User;

namespace Shopbe.Application.Tests.User;

public class CreateUserHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ICurrentUser> _currentUser;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IMapper> _mapper;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);
        _userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        _mapper = new Mock<IMapper>(MockBehavior.Strict);

        _unitOfWork.SetupGet(u => u.Users).Returns(_userRepository.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new CreateUserHandler(_unitOfWork.Object, _currentUser.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenKeycloakIdIsMissing()
    {
        _currentUser.SetupGet(c => c.KeycloakId).Returns((string?)null);

        var command = new CreateUserCommand(new UserRequestDto
        {
            FullName = "John Doe",
            Email = "john@example.com"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailIsMissing()
    {
        _currentUser.SetupGet(c => c.KeycloakId).Returns("kc-123");
        _currentUser.SetupGet(c => c.Email).Returns((string?)null);
        _currentUser.SetupGet(c => c.FullName).Returns("John Doe");

        var command = new CreateUserCommand(new UserRequestDto
        {
            FullName = "John Doe",
            Email = ""
        });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenFullNameIsMissing()
    {
        _currentUser.SetupGet(c => c.KeycloakId).Returns("kc-123");
        _currentUser.SetupGet(c => c.Email).Returns("john@example.com");
        _currentUser.SetupGet(c => c.FullName).Returns((string?)null);

        var command = new CreateUserCommand(new UserRequestDto
        {
            FullName = "",
            Email = "john@example.com"
        });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingUser_WhenKeycloakIdExists()
    {
        var keycloakId = "kc-123";
        var existingUser = new DomainUser
        {
            Id = Guid.NewGuid(),
            KeycloakId = keycloakId,
            Email = "old@example.com",
            FullName = "Old Name",
            PhoneNumber = null,
            Gender = null,
        };

        _currentUser.SetupGet(c => c.KeycloakId).Returns(keycloakId);
        _currentUser.SetupGet(c => c.Email).Returns("new@example.com");
        _currentUser.SetupGet(c => c.FullName).Returns("New Name");

        _userRepository
            .Setup(r => r.GetUserByKeycloakIdAsync(keycloakId))
            .ReturnsAsync(existingUser);

        _userRepository
            .Setup(r => r.UpdateUserAsync(It.IsAny<DomainUser>()))
            .Returns(Task.CompletedTask);

        var command = new CreateUserCommand(new UserRequestDto
        {
            FullName = "Request Name",
            Email = "request@example.com",
            PhoneNumber = "0912345678",
            Gender = "male",
        });

        _mapper
            .Setup(m => m.Map<UserResponseDto>(existingUser))
            .Returns(new UserResponseDto
            {
                Id = existingUser.Id,
                KeycloakId = existingUser.KeycloakId,
                Email = existingUser.Email,
                FullName = existingUser.FullName,
                PhoneNumber = existingUser.PhoneNumber,
                Gender = existingUser.Gender,
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("new@example.com", existingUser.Email);  // claim takes precedence
        Assert.Equal("New Name", existingUser.FullName);      // claim takes precedence
        Assert.Equal("0912345678", existingUser.PhoneNumber);
        Assert.Equal("male", existingUser.Gender);

        _userRepository.Verify(r => r.UpdateUserAsync(existingUser), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateNewUser_WhenNotExists()
    {
        var keycloakId = "kc-new";

        _currentUser.SetupGet(c => c.KeycloakId).Returns(keycloakId);
        _currentUser.SetupGet(c => c.Email).Returns("new@example.com");
        _currentUser.SetupGet(c => c.FullName).Returns("New User");

        _userRepository
            .Setup(r => r.GetUserByKeycloakIdAsync(keycloakId))
            .ReturnsAsync((DomainUser?)null);

        _userRepository
            .Setup(r => r.GetUserByEmailAsync("new@example.com"))
            .ReturnsAsync((DomainUser?)null);

        _userRepository
            .Setup(r => r.CreateUserAsync(It.IsAny<DomainUser>()))
            .Returns(Task.CompletedTask);

        _mapper
            .Setup(m => m.Map<UserResponseDto>(It.IsAny<DomainUser>()))
            .Returns((DomainUser u) => new UserResponseDto
            {
                Id = u.Id,
                KeycloakId = u.KeycloakId,
                Email = u.Email,
                FullName = u.FullName,
                Country = u.Country,
            });

        var command = new CreateUserCommand(new UserRequestDto
        {
            FullName = "New User",
            Email = "new@example.com",
            Country = "Vietnam",
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("new@example.com", result.Email);
        Assert.Equal("New User", result.FullName);

        _userRepository.Verify(r => r.CreateUserAsync(It.IsAny<DomainUser>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailConflictsWithDifferentKeycloakId()
    {
        var keycloakId = "kc-new";
        var conflictingUser = new DomainUser
        {
            Id = Guid.NewGuid(),
            KeycloakId = "kc-other",
            Email = "existing@example.com",
            FullName = "Existing User",
        };

        _currentUser.SetupGet(c => c.KeycloakId).Returns(keycloakId);
        _currentUser.SetupGet(c => c.Email).Returns("existing@example.com");
        _currentUser.SetupGet(c => c.FullName).Returns("New User");

        _userRepository
            .Setup(r => r.GetUserByKeycloakIdAsync(keycloakId))
            .ReturnsAsync((DomainUser?)null);

        _userRepository
            .Setup(r => r.GetUserByEmailAsync("existing@example.com"))
            .ReturnsAsync(conflictingUser);

        var command = new CreateUserCommand(new UserRequestDto
        {
            FullName = "New User",
            Email = "existing@example.com",
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));

        Assert.Contains("already in use", ex.Message);
    }

    [Fact]
    public async Task Handle_ShouldRecoverFromConcurrentCreate_OnException()
    {
        var keycloakId = "kc-concurrent";

        _currentUser.SetupGet(c => c.KeycloakId).Returns(keycloakId);
        _currentUser.SetupGet(c => c.Email).Returns("concurrent@example.com");
        _currentUser.SetupGet(c => c.FullName).Returns("Concurrent User");

        var recoveredUser = new DomainUser
        {
            Id = Guid.NewGuid(),
            KeycloakId = keycloakId,
            Email = "concurrent@example.com",
            FullName = "Concurrent User",
        };

        _userRepository
            .SetupSequence(r => r.GetUserByKeycloakIdAsync(keycloakId))
            .ReturnsAsync((DomainUser?)null)    // first call: not found
            .ReturnsAsync(recoveredUser);        // second call: found (concurrent create)

        _userRepository
            .Setup(r => r.GetUserByEmailAsync("concurrent@example.com"))
            .ReturnsAsync((DomainUser?)null);

        _userRepository
            .Setup(r => r.CreateUserAsync(It.IsAny<DomainUser>()))
            .ThrowsAsync(new Exception("DB constraint violation"));

        _mapper
            .Setup(m => m.Map<UserResponseDto>(recoveredUser))
            .Returns(new UserResponseDto
            {
                Id = recoveredUser.Id,
                KeycloakId = recoveredUser.KeycloakId,
                Email = recoveredUser.Email,
                FullName = recoveredUser.FullName,
            });

        var command = new CreateUserCommand(new UserRequestDto
        {
            FullName = "Concurrent User",
            Email = "concurrent@example.com",
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("concurrent@example.com", result.Email);
        Assert.Equal("Concurrent User", result.FullName);
    }

    [Fact]
    public async Task Handle_ShouldReThrow_WhenConcurrentCreateFailsAndReReadReturnsNull()
    {
        var keycloakId = "kc-fail";

        _currentUser.SetupGet(c => c.KeycloakId).Returns(keycloakId);
        _currentUser.SetupGet(c => c.Email).Returns("fail@example.com");
        _currentUser.SetupGet(c => c.FullName).Returns("Fail User");

        _userRepository
            .Setup(r => r.GetUserByKeycloakIdAsync(keycloakId))
            .ReturnsAsync((DomainUser?)null);

        _userRepository
            .Setup(r => r.GetUserByEmailAsync("fail@example.com"))
            .ReturnsAsync((DomainUser?)null);

        _userRepository
            .Setup(r => r.CreateUserAsync(It.IsAny<DomainUser>()))
            .ThrowsAsync(new Exception("DB constraint violation"));

        _userRepository
            .Setup(r => r.GetUserByKeycloakIdAsync(keycloakId))
            .ReturnsAsync((DomainUser?)null);

        var command = new CreateUserCommand(new UserRequestDto
        {
            FullName = "Fail User",
            Email = "fail@example.com",
        });

        await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}

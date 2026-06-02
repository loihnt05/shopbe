using AutoMapper;
using Moq;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Application.Common.Interfaces.IUser;
using Shopbe.Application.User.Users.Dtos;
using Shopbe.Application.User.Users.Queries.GetUserByKeycloakId;
using DomainUser = Shopbe.Domain.Entities.User.User;

namespace Shopbe.Application.Tests.User;

public class GetUserByKeycloakHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IMapper> _mapper;
    private readonly GetUserByKeycloakId _handler;

    public GetUserByKeycloakHandlerTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        _mapper = new Mock<IMapper>(MockBehavior.Strict);

        _unitOfWork.SetupGet(u => u.Users).Returns(_userRepository.Object);

        _handler = new GetUserByKeycloakId(_unitOfWork.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUser_WhenFound()
    {
        var keycloakId = "kc-123";
        var user = new DomainUser
        {
            Id = Guid.NewGuid(),
            KeycloakId = keycloakId,
            Email = "john@example.com",
            FullName = "John Doe",
            PhoneNumber = "0912345678",
            Gender = "male",
        };

        _userRepository
            .Setup(r => r.GetUserByKeycloakIdAsync(keycloakId))
            .ReturnsAsync(user);

        _mapper
            .Setup(m => m.Map<UserResponseDto>(user))
            .Returns(new UserResponseDto
            {
                Id = user.Id,
                KeycloakId = user.KeycloakId,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
            });

        var result = await _handler.Handle(
            new GetUserByKeycloakQuery(new UserQueryDto { KeycloakId = keycloakId }),
            CancellationToken.None);

        Assert.Equal(keycloakId, result.KeycloakId);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("John Doe", result.FullName);
        Assert.Equal("0912345678", result.PhoneNumber);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenKeycloakIdIsNull()
    {
        var query = new GetUserByKeycloakQuery(new UserQueryDto { KeycloakId = null });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserNotFound()
    {
        var keycloakId = "kc-missing";

        _userRepository
            .Setup(r => r.GetUserByKeycloakIdAsync(keycloakId))
            .ReturnsAsync((DomainUser?)null);

        var query = new GetUserByKeycloakQuery(new UserQueryDto { KeycloakId = keycloakId });

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }
}

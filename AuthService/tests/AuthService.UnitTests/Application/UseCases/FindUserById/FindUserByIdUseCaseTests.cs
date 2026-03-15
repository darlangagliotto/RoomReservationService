using AuthService.Application.UseCases.FindUserById;
using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AuthService.UnitTests.Application.UseCases.FindUserById;

public class FindUserByIdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenUserExists_ReturnsUserData()
    {
        var repository = new Mock<IUserRepository>();
        var user = new User("Darlan", new Email("darlan@example.com"), "hashed-password");
        repository.Setup(repo => repo.GetByIdAsync(user.Id)).ReturnsAsync(user);
        var useCase = new FindUserByIdUseCase(repository.Object);
        var request = new FindUserByIdRequest(user.Id);

        var result = await useCase.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(user.Id);
        result.Value.Name.Should().Be("Darlan");
        result.Value.Email.Should().Be("darlan@example.com");
        result.Value.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        var repository = new Mock<IUserRepository>();
        var userId = Guid.NewGuid();
        repository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User?)null);
        var useCase = new FindUserByIdUseCase(repository.Object);
        var request = new FindUserByIdRequest(userId);

        var result = await useCase.ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Usuário não encontrado.");
    }
}
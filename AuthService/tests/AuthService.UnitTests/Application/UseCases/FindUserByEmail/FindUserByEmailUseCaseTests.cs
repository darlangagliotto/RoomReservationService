using AuthService.Application.UseCases.FindUserByEmail;
using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AuthService.UnitTests.Application.UseCases.FindUserByEmail;

public class FindUserByEmailUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenEmailIsEmpty_ReturnsFailure()
    {
        var repository = new Mock<IUserRepository>();
        var useCase = new FindUserByEmailUseCase(repository.Object);
        var request = new FindUserByEmailRequest(string.Empty);

        var result = await useCase.ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("É necessário informar o e-mail.");
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserExists_ReturnsUserData()
    {
        var repository = new Mock<IUserRepository>();
        var user = new User("Darlan", new Email("darlan@example.com"), "hashed-password");
        repository.Setup(repo => repo.GetByEmailAsync("darlan@example.com")).ReturnsAsync(user);
        var useCase = new FindUserByEmailUseCase(repository.Object);
        var request = new FindUserByEmailRequest("darlan@example.com");

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
        repository.Setup(repo => repo.GetByEmailAsync("missing@example.com")).ReturnsAsync((User?)null);
        var useCase = new FindUserByEmailUseCase(repository.Object);
        var request = new FindUserByEmailRequest("missing@example.com");

        var result = await useCase.ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Usuário não encontrado.");
    }
}
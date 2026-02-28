using Xunit;
using FluentAssertions;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;

namespace AuthService.UnitTests.Domain.Entities
{
    public class UserTests
    {
        [Fact]
        public void Should_Create_User_When_Data_Is_Valid()
        {
            // Arrange
            var name = "João";
            var email = new Email("joao@email.com");
            var password = "123456789";

            // Act
            var user = new User(name, email, password);

            // Assert
            user.Should().NotBeNull();
            user.Name.Should().Be(name);
            user.Email.Should().Be(email);
            user.PasswordHash.Should().Be(password);
        }

        [Fact]
        public void Should_Generate_Id_When_User_Is_Created()
        {
            // Act
            var user = CreateValidUser();

            // Assert
            user.Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void Should_User_Start_As_Unblocked()
        {
            // Arrange
            var user = CreateValidUser();            

            // Assert
            user.IsBlocked.Should().BeFalse();
        }

        [Fact]
        public void Should_Block_User()
        {
            // Arrange
            var user = CreateValidUser();

            // Act
            user.Block();

            // Assert
            user.IsBlocked.Should().BeTrue();
        }

        [Fact]
        public void Should_Unblock_User()
        {
            // Arrange
            var user = CreateValidUser();
            user.Block();           

            // Act
            user.Unblock();

            // Assert
            user.IsBlocked.Should().BeFalse();
        }

        [Fact]
        public void Should_Throw_When_Email_Is_Null()
        {
            // Arrange
            var name = "João";
            string passwordHash = "123456";

            // Act
            Action act = () => new User(name, null!, passwordHash);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("email");
        }

        private static User CreateValidUser() 
        => new User(            
            "João",
            new Email("joao@email.com"),
            "123456789"
        );
    }
}
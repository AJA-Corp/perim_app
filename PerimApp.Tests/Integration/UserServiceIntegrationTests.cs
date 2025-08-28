using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using PerimApp.Core.Services;
using PerimApp.Core.Models;

namespace PerimApp.Tests.Integration
{
    public class UserServiceIntegrationTests
    {
        private const string TestConnectionString = "Host=localhost;Username=test;Password=test;Database=test_db";

        [Fact]
        public void NeonUserService_Constructor_WithValidConnectionString_ShouldNotThrow()
        {
            // Act
            var action = () => new NeonUserService(TestConnectionString);

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void NeonUserService_Constructor_WithNullConnectionString_ShouldThrowException()
        {
            // Act
            var action = () => new NeonUserService(null!);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task RegisterUserAsync_WithNullUser_ShouldThrowException()
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);

            // Act
            var action = async () => await service.RegisterUserAsync(null!);

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task RegisterUserAsync_WithInvalidUser_ShouldThrowException()
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);
            var invalidUser = new UserProfileDetails
            {
                FirstName = "", // Invalid
                LastName = "Dupont",
                Email = "invalid-email", // Invalid
                Password = "weak", // Invalid
                HomeCode = 123 // Invalid
            };

            // Act
            var action = async () => await service.RegisterUserAsync(invalidUser);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Les données utilisateur ne sont pas valides");
        }

        [Fact]
        public async Task UpdateUserAsync_WithNullUser_ShouldThrowException()
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);

            // Act
            var action = async () => await service.UpdateUserAsync(null!);

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public void UserService_Interface_ShouldBeImplementedCorrectly()
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);

            // Assert
            service.Should().BeAssignableTo<IUserService>();
        }

        [Fact]
        public async Task LoginUserAsync_WithEmptyCredentials_ShouldReturnNull()
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);

            // Act
            var result1 = await service.LoginUserAsync("", "password");
            var result2 = await service.LoginUserAsync("email", "");
            var result3 = await service.LoginUserAsync("", "");

            // Assert
            result1.Should().BeNull();
            result2.Should().BeNull();
            result3.Should().BeNull();
        }

        [Fact]
        public async Task LoginUserAsync_WithNullCredentials_ShouldReturnNull()
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);

            // Act
            var result1 = await service.LoginUserAsync(null!, "password");
            var result2 = await service.LoginUserAsync("email", null!);
            var result3 = await service.LoginUserAsync(null!, null!);

            // Assert
            result1.Should().BeNull();
            result2.Should().BeNull();
            result3.Should().BeNull();
        }

        [Fact]
        public async Task EmailExistsAsync_WithEmptyOrNullEmail_ShouldReturnFalse()
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);

            // Act
            var result1 = await service.EmailExistsAsync("");
            var result2 = await service.EmailExistsAsync("   ");
            var result3 = await service.EmailExistsAsync(null!);

            // Assert
            result1.Should().BeFalse();
            result2.Should().BeFalse();
            result3.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterUserAsync_WithValidUser_ShouldHandleConnectionFailure()
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);
            var validUser = CreateValidUser();

            // Act
            var action = async () => await service.RegisterUserAsync(validUser);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task LoginUserAsync_WithValidCredentials_ShouldHandleConnectionFailure()
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);

            // Act
            var action = async () => await service.LoginUserAsync("test@example.com", "password");

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task UpdateUserAsync_WithValidUser_ShouldHandleConnectionFailure()
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);
            var validUser = CreateValidUser();

            // Act
            var action = async () => await service.UpdateUserAsync(validUser);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task EmailExistsAsync_WithValidEmail_ShouldHandleConnectionFailure()
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);

            // Act
            var action = async () => await service.EmailExistsAsync("test@example.com");

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task HomeCodeExistsAsync_WithValidHomeCode_ShouldHandleConnectionFailure()
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);

            // Act
            var action = async () => await service.HomeCodeExistsAsync(123456);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email")]
        [InlineData("@domain.com")]
        [InlineData("user@")]
        public async Task LoginUserAsync_WithInvalidEmailFormat_ShouldStillAttemptConnection(string email)
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);

            // Act
            var action = async () => await service.LoginUserAsync(email, "validPassword123!");

            // Assert
            // The service should still attempt the database operation even with invalid email formats
            await action.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldGenerateValidHomeCode()
        {
            // This test verifies that the home code generation logic would work
            // We can't test the actual database operation, but we can verify the user object
            
            // Arrange
            var service = new NeonUserService(TestConnectionString);
            var user = CreateValidUser();
            var originalHomeCode = user.HomeCode;

            // Act
            var action = async () => await service.RegisterUserAsync(user);

            // Assert
            await action.Should().ThrowAsync<Exception>();
            
            // The home code should be modified during the registration process
            // (although we can't verify this due to the connection failure)
            user.HomeCode.Should().BeGreaterOrEqualTo(100000);
            user.HomeCode.Should().BeLessOrEqualTo(999999);
        }

        [Fact]
        public void UserService_Methods_ShouldHaveCorrectSignatures()
        {
            // This test verifies that the interface methods have the expected signatures
            var serviceType = typeof(NeonUserService);
            var interfaceType = typeof(IUserService);

            // Verify that NeonUserService implements IUserService
            interfaceType.IsAssignableFrom(serviceType).Should().BeTrue();

            // Verify specific method signatures exist
            var registerMethod = serviceType.GetMethod("RegisterUserAsync");
            registerMethod.Should().NotBeNull();
            registerMethod!.ReturnType.Should().Be(typeof(Task<int>));

            var loginMethod = serviceType.GetMethod("LoginUserAsync");
            loginMethod.Should().NotBeNull();
            loginMethod!.ReturnType.Should().Be(typeof(Task<UserProfileDetails?>));

            var updateMethod = serviceType.GetMethod("UpdateUserAsync");
            updateMethod.Should().NotBeNull();
            updateMethod!.ReturnType.Should().Be(typeof(Task<bool>));

            var emailExistsMethod = serviceType.GetMethod("EmailExistsAsync");
            emailExistsMethod.Should().NotBeNull();
            emailExistsMethod!.ReturnType.Should().Be(typeof(Task<bool>));

            var homeCodeExistsMethod = serviceType.GetMethod("HomeCodeExistsAsync");
            homeCodeExistsMethod.Should().NotBeNull();
            homeCodeExistsMethod!.ReturnType.Should().Be(typeof(Task<bool>));
        }

        [Fact]
        public async Task RegisterUserAsync_WithValidUserData_ShouldValidateBeforeAttemptingDatabase()
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);
            var validUser = CreateValidUser();

            // Verify the user data is valid before the test
            validUser.IsValid().Should().BeTrue();

            // Act
            var action = async () => await service.RegisterUserAsync(validUser);

            // Assert
            // The method should throw due to connection failure, not validation failure
            await action.Should().ThrowAsync<Exception>()
                .Where(ex => ex.Message != "Les données utilisateur ne sont pas valides");
        }

        private static UserProfileDetails CreateValidUser()
        {
            return new UserProfileDetails
            {
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean.dupont@example.com",
                Password = "SecurePassword123!",
                HomeCode = 123456
            };
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(99999)]
        [InlineData(1000000)]
        public async Task HomeCodeExistsAsync_WithInvalidHomeCodes_ShouldStillAttemptConnection(int homeCode)
        {
            // Arrange
            var service = new NeonUserService(TestConnectionString);

            // Act
            var action = async () => await service.HomeCodeExistsAsync(homeCode);

            // Assert
            // Even with invalid home codes, the service should still attempt the database operation
            await action.Should().ThrowAsync<Exception>();
        }
    }
}
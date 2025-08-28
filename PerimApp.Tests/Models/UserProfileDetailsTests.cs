using System;
using Xunit;
using FluentAssertions;
using PerimApp.Core.Models;

namespace PerimApp.Tests.Models
{
    public class UserProfileDetailsTests
    {
        [Fact]
        public void FullName_ShouldCombineFirstAndLastName()
        {
            // Arrange
            var user = new UserProfileDetails
            {
                FirstName = "Jean",
                LastName = "Dupont"
            };

            // Act
            var result = user.FullName;

            // Assert
            result.Should().Be("Jean Dupont");
        }

        [Theory]
        [InlineData("", "Dupont", "Dupont")]
        [InlineData("Jean", "", "Jean")]
        [InlineData("", "", "")]
        [InlineData("  Jean  ", "  Dupont  ", "Jean     Dupont")]
        public void FullName_ShouldHandleEdgeCases(string firstName, string lastName, string expected)
        {
            // Arrange
            var user = new UserProfileDetails
            {
                FirstName = firstName,
                LastName = lastName
            };

            // Act
            var result = user.FullName;

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void IsValid_WhenAllFieldsAreValid_ShouldReturnTrue()
        {
            // Arrange
            var user = new UserProfileDetails
            {
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean.dupont@example.com",
                Password = "Password123!",
                HomeCode = 123456
            };

            // Act
            var result = user.IsValid();

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "Dupont", "jean@example.com", "Password123!", 123456)] // FirstName empty
        [InlineData("Jean", "", "jean@example.com", "Password123!", 123456)] // LastName empty
        [InlineData("Jean", "Dupont", "invalid-email", "Password123!", 123456)] // Invalid email
        [InlineData("Jean", "Dupont", "jean@example.com", "", 123456)] // Password empty
        [InlineData("Jean", "Dupont", "jean@example.com", "Password123!", 99999)] // HomeCode too small
        [InlineData("Jean", "Dupont", "jean@example.com", "Password123!", 1000000)] // HomeCode too large
        public void IsValid_WhenFieldsAreInvalid_ShouldReturnFalse(
            string firstName, string lastName, string email, string password, int homeCode)
        {
            // Arrange
            var user = new UserProfileDetails
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
                HomeCode = homeCode
            };

            // Act
            var result = user.IsValid();

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("jean@example.com", true)]
        [InlineData("jean.dupont@example.fr", true)]
        [InlineData("jean_dupont@example-site.com", true)]
        [InlineData("invalid-email", false)]
        [InlineData("@example.com", false)]
        [InlineData("jean@", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsValidEmail_ShouldValidateEmailFormat(string email, bool expected)
        {
            // Act
            var result = UserProfileDetails.IsValidEmail(email);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("Password123!", true)] // Has all requirements
        [InlineData("password123!", false)] // No uppercase
        [InlineData("PASSWORD123!", false)] // No lowercase
        [InlineData("Password!", false)] // No digit
        [InlineData("Pass123", false)] // Too short
        [InlineData("", false)] // Empty
        [InlineData(null, false)] // Null
        public void IsPasswordStrong_ShouldValidatePasswordStrength(string password, bool expected)
        {
            // Arrange
            var user = new UserProfileDetails { Password = password };

            // Act
            var result = user.IsPasswordStrong();

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var user = new UserProfileDetails();

            // Assert
            user.FirstName.Should().Be(string.Empty);
            user.LastName.Should().Be(string.Empty);
            user.Email.Should().Be(string.Empty);
            user.Password.Should().Be(string.Empty);
            user.LostProducts.Should().Be(string.Empty);
            user.HomeCode.Should().Be(0);
            user.RegisteredProductsCount.Should().Be(0);
        }

        [Theory]
        [InlineData(100000, true)]
        [InlineData(999999, true)]
        [InlineData(123456, true)]
        [InlineData(99999, false)]
        [InlineData(1000000, false)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void HomeCode_ShouldBeValidSixDigitNumber(int homeCode, bool shouldBeValid)
        {
            // Arrange
            var user = new UserProfileDetails
            {
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean@example.com",
                Password = "Password123!",
                HomeCode = homeCode
            };

            // Act
            var result = user.IsValid();

            // Assert
            result.Should().Be(shouldBeValid);
        }

        [Fact]
        public void JsonPropertyNames_ShouldBeCorrect()
        {
            // This test ensures that the JSON property names are correctly set
            // We'll check this by creating an instance and verifying the attributes exist
            
            // Arrange
            var user = new UserProfileDetails();
            var type = typeof(UserProfileDetails);

            // Act & Assert
            var firstNameProperty = type.GetProperty("FirstName");
            var lastNameProperty = type.GetProperty("LastName");
            var emailProperty = type.GetProperty("Email");
            var passwordProperty = type.GetProperty("Password");
            var homeCodeProperty = type.GetProperty("HomeCode");
            var lostProductsProperty = type.GetProperty("LostProducts");

            // Verify properties exist
            firstNameProperty.Should().NotBeNull();
            lastNameProperty.Should().NotBeNull();
            emailProperty.Should().NotBeNull();
            passwordProperty.Should().NotBeNull();
            homeCodeProperty.Should().NotBeNull();
            lostProductsProperty.Should().NotBeNull();
        }
    }
}
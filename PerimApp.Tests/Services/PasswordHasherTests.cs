using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using PerimApp.Core.Services;

namespace PerimApp.Tests.Services
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashPassword_WithValidPassword_ShouldReturnHash()
        {
            // Arrange
            const string password = "TestPassword123!";

            // Act
            var hash = PasswordHasher.HashPassword(password);

            // Assert
            hash.Should().NotBeNullOrEmpty();
            hash.Should().NotBe(password); // Hash should be different from original
            hash.Length.Should().BeGreaterThan(password.Length); // Hash is typically longer
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void HashPassword_WithEmptyOrWhitespacePassword_ShouldThrowException(string password)
        {
            // Act & Assert
            var action = () => PasswordHasher.HashPassword(password);
            action.Should().Throw<ArgumentException>()
                .WithMessage("Le mot de passe ne peut pas être vide*");
        }

        [Fact]
        public void HashPassword_WithNullPassword_ShouldThrowException()
        {
            // Act & Assert
            var action = () => PasswordHasher.HashPassword(null!);
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            const string password = "TestPassword123!";
            var hash = PasswordHasher.HashPassword(password);

            // Act
            var result = PasswordHasher.VerifyPassword(password, hash);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
        {
            // Arrange
            const string correctPassword = "TestPassword123!";
            const string incorrectPassword = "WrongPassword123!";
            var hash = PasswordHasher.HashPassword(correctPassword);

            // Act
            var result = PasswordHasher.VerifyPassword(incorrectPassword, hash);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("", "validHash")]
        [InlineData("validPassword", "")]
        [InlineData("", "")]
        [InlineData(null, "validHash")]
        [InlineData("validPassword", null)]
        public void VerifyPassword_WithEmptyOrNullInputs_ShouldReturnFalse(string password, string hash)
        {
            // Act
            var result = PasswordHasher.VerifyPassword(password, hash);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_WithInvalidHash_ShouldReturnFalse()
        {
            // Arrange
            const string password = "TestPassword123!";
            const string invalidHash = "not-a-valid-hash";

            // Act
            var result = PasswordHasher.VerifyPassword(password, invalidHash);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("Password123!", true)] // All requirements met
        [InlineData("password123!", false)] // No uppercase
        [InlineData("PASSWORD123!", false)] // No lowercase
        [InlineData("Password!", false)] // No digit
        [InlineData("Password123", false)] // No special character
        [InlineData("Pass1!", false)] // Too short
        [InlineData("", false)] // Empty
        [InlineData(null, false)] // Null
        public void IsPasswordStrong_ShouldValidatePasswordStrength(string password, bool expected)
        {
            // Act
            var result = PasswordHasher.IsPasswordStrong(password);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void GetPasswordRecommendations_WithNullPassword_ShouldReturnEmptyWarning()
        {
            // Act
            var recommendations = PasswordHasher.GetPasswordRecommendations(null);

            // Assert
            recommendations.Should().ContainSingle("Le mot de passe ne peut pas être vide");
        }

        [Fact]
        public void GetPasswordRecommendations_WithEmptyPassword_ShouldReturnEmptyWarning()
        {
            // Act
            var recommendations = PasswordHasher.GetPasswordRecommendations("");

            // Assert
            recommendations.Should().ContainSingle("Le mot de passe ne peut pas être vide");
        }

        [Fact]
        public void GetPasswordRecommendations_WithWeakPassword_ShouldReturnAllRecommendations()
        {
            // Arrange
            const string weakPassword = "weak"; // 4 characters, no uppercase, no digit, no special char

            // Act
            var recommendations = PasswordHasher.GetPasswordRecommendations(weakPassword);

            // Assert
            recommendations.Should().HaveCount(4);
            recommendations.Should().Contain("Le mot de passe doit contenir au moins 8 caractères");
            recommendations.Should().Contain("Le mot de passe doit contenir au moins une majuscule");
            recommendations.Should().Contain("Le mot de passe doit contenir au moins un chiffre");
            recommendations.Should().Contain("Le mot de passe doit contenir au moins un caractère spécial");
        }

        [Theory]
        [InlineData("password", new[] { 
            "Le mot de passe doit contenir au moins une majuscule",
            "Le mot de passe doit contenir au moins un chiffre",
            "Le mot de passe doit contenir au moins un caractère spécial"
        })]
        [InlineData("PASSWORD", new[] { 
            "Le mot de passe doit contenir au moins une minuscule",
            "Le mot de passe doit contenir au moins un chiffre",
            "Le mot de passe doit contenir au moins un caractère spécial"
        })]
        [InlineData("Password", new[] { 
            "Le mot de passe doit contenir au moins un chiffre",
            "Le mot de passe doit contenir au moins un caractère spécial"
        })]
        [InlineData("Password1", new[] { 
            "Le mot de passe doit contenir au moins un caractère spécial"
        })]
        [InlineData("Password123", new[] { 
            "Le mot de passe doit contenir au moins un caractère spécial"
        })]
        public void GetPasswordRecommendations_ShouldReturnSpecificRecommendations(
            string password, string[] expectedRecommendations)
        {
            // Act
            var recommendations = PasswordHasher.GetPasswordRecommendations(password);

            // Assert
            recommendations.Should().HaveCount(expectedRecommendations.Length);
            foreach (var expectedRecommendation in expectedRecommendations)
            {
                recommendations.Should().Contain(expectedRecommendation);
            }
        }

        [Fact]
        public void GetPasswordRecommendations_WithStrongPassword_ShouldReturnEmptyList()
        {
            // Arrange
            const string strongPassword = "StrongPassword123!";

            // Act
            var recommendations = PasswordHasher.GetPasswordRecommendations(strongPassword);

            // Assert
            recommendations.Should().BeEmpty();
        }

        [Fact]
        public void HashPassword_DifferentPasswordsShouldProduceDifferentHashes()
        {
            // Arrange
            const string password1 = "Password123!";
            const string password2 = "Password124!";

            // Act
            var hash1 = PasswordHasher.HashPassword(password1);
            var hash2 = PasswordHasher.HashPassword(password2);

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void HashPassword_SamePasswordShouldProduceDifferentHashes()
        {
            // This tests that salt is properly used
            // Arrange
            const string password = "Password123!";

            // Act
            var hash1 = PasswordHasher.HashPassword(password);
            var hash2 = PasswordHasher.HashPassword(password);

            // Assert
            hash1.Should().NotBe(hash2); // Argon2 should use different salts
            
            // But both should verify against the original password
            PasswordHasher.VerifyPassword(password, hash1).Should().BeTrue();
            PasswordHasher.VerifyPassword(password, hash2).Should().BeTrue();
        }
    }
}
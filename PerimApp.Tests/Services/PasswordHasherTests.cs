using System;
using Xunit;
using Isopoh.Cryptography.Argon2;

namespace PerimApp.Tests.Services
{
    // Test version of PasswordHasher to avoid MAUI dependencies
    public static class PasswordHasherTestClass
    {
        public static string HashPassword(string password)
        {
            return Argon2.Hash(password);
        }

        public static bool VerifyPassword(string hashedPassword, string enteredPassword)
        {
            return Argon2.Verify(hashedPassword, enteredPassword);
        }
    }

    public class PasswordHasherTests
    {
        [Fact]
        public void HashPassword_ShouldReturnNonEmptyString()
        {
            // Arrange
            string password = "TestPassword123";

            // Act
            string hashedPassword = PasswordHasherTestClass.HashPassword(password);

            // Assert
            Assert.NotNull(hashedPassword);
            Assert.NotEmpty(hashedPassword);
        }

        [Fact]
        public void HashPassword_ShouldReturnDifferentHashesForSamePassword()
        {
            // Arrange
            string password = "TestPassword123";

            // Act
            string hash1 = PasswordHasherTestClass.HashPassword(password);
            string hash2 = PasswordHasherTestClass.HashPassword(password);

            // Assert
            Assert.NotEqual(hash1, hash2); // Argon2 includes salt, so hashes should be different
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatches()
        {
            // Arrange
            string password = "TestPassword123";
            string hashedPassword = PasswordHasherTestClass.HashPassword(password);

            // Act
            bool result = PasswordHasherTestClass.VerifyPassword(hashedPassword, password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatch()
        {
            // Arrange
            string correctPassword = "TestPassword123";
            string wrongPassword = "WrongPassword456";
            string hashedPassword = PasswordHasherTestClass.HashPassword(correctPassword);

            // Act
            bool result = PasswordHasherTestClass.VerifyPassword(hashedPassword, wrongPassword);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("short")]
        [InlineData("thisIsAVeryLongPasswordThatShouldStillWork")]
        [InlineData("PasswordWith123Numbers")]
        [InlineData("PasswordWith!@#SpecialChars")]
        public void HashPassword_ShouldWorkWithVariousPasswordLengths(string password)
        {
            // Act
            string hashedPassword = PasswordHasherTestClass.HashPassword(password);

            // Assert
            Assert.NotNull(hashedPassword);
            Assert.NotEmpty(hashedPassword);
            
            // Verify the hash can be validated
            bool isValid = PasswordHasherTestClass.VerifyPassword(hashedPassword, password);
            Assert.True(isValid);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenHashIsInvalid()
        {
            // Arrange
            string invalidHash = "ThisIsNotAValidArgon2Hash";
            string password = "TestPassword123";

            // Act
            bool result = PasswordHasherTestClass.VerifyPassword(invalidHash, password);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HashPassword_ShouldProduceArgon2Format()
        {
            // Arrange
            string password = "TestPassword123";

            // Act
            string hashedPassword = PasswordHasherTestClass.HashPassword(password);

            // Assert
            Assert.StartsWith("$argon2", hashedPassword);
        }
    }
}
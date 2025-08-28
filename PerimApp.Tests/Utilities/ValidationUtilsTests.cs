using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using PerimApp.Core.Utilities;
using PerimApp.Core.Models;

namespace PerimApp.Tests.Utilities
{
    public class ValidationUtilsTests
    {
        [Theory]
        [InlineData(12345678, true)]
        [InlineData(1234567890123, true)]
        [InlineData(87654321, true)]
        [InlineData(1234567, false)] // Too short
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void IsValidBarcode_ShouldValidateCorrectly(long barcode, bool expected)
        {
            // Act
            var result = ValidationUtils.IsValidBarcode(barcode);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(999, true)]
        [InlineData(500, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(1000, false)]
        public void IsValidQuantity_ShouldValidateCorrectly(int quantity, bool expected)
        {
            // Act
            var result = ValidationUtils.IsValidQuantity(quantity);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void IsValidDlc_WithValidDates_ShouldReturnTrue()
        {
            // Arrange
            var validDates = new[]
            {
                DateTime.Today,
                DateTime.Today.AddDays(1),
                DateTime.Today.AddMonths(6),
                DateTime.Today.AddYears(5)
            };

            // Act & Assert
            foreach (var date in validDates)
            {
                ValidationUtils.IsValidDlc(date).Should().BeTrue($"Date {date:yyyy-MM-dd} should be valid");
            }
        }

        [Fact]
        public void IsValidDlc_WithInvalidDates_ShouldReturnFalse()
        {
            // Arrange
            var invalidDates = new[]
            {
                DateTime.Today.AddDays(-1), // Yesterday
                DateTime.Today.AddYears(-1), // Last year
                DateTime.Today.AddYears(11), // Too far in future
                DateTime.MinValue,
                DateTime.MaxValue
            };

            // Act & Assert
            foreach (var date in invalidDates)
            {
                ValidationUtils.IsValidDlc(date).Should().BeFalse($"Date {date:yyyy-MM-dd} should be invalid");
            }
        }

        [Theory]
        [InlineData("https://example.com/image.jpg", true)]
        [InlineData("http://example.com/image.png", true)]
        [InlineData("", true)] // Empty is allowed
        [InlineData(null, true)] // Null is allowed
        [InlineData("   ", true)] // Whitespace is allowed
        [InlineData("ftp://example.com/image.jpg", false)]
        [InlineData("not-a-url", false)]
        [InlineData("javascript:alert('xss')", false)]
        public void IsValidImageUrl_ShouldValidateCorrectly(string urlImage, bool expected)
        {
            // Act
            var result = ValidationUtils.IsValidImageUrl(urlImage);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("Product Name", "Product Name")]
        [InlineData("  Product Name  ", "Product Name")]
        [InlineData("Product  Name", "Product Name")]
        [InlineData("Product\nName", "Product Name")]
        [InlineData("Product\tName", "Product Name")]
        [InlineData("", "")]
        [InlineData(null, "")]
        [InlineData("   ", "")]
        public void CleanProductName_ShouldCleanCorrectly(string input, string expected)
        {
            // Act
            var result = ValidationUtils.CleanProductName(input);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("Fruits", true)]
        [InlineData("fruits", true)] // Case insensitive
        [InlineData("LÉGUMES", true)]
        [InlineData("Viandes", true)]
        [InlineData("Autre", true)]
        [InlineData("InvalidCategory", false)]
        [InlineData("", true)] // Empty is allowed
        [InlineData(null, true)] // Null is allowed
        public void IsValidCategory_ShouldValidateCorrectly(string category, bool expected)
        {
            // Act
            var result = ValidationUtils.IsValidCategory(category);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void GetProductValidationErrors_WithValidProduct_ShouldReturnEmptyList()
        {
            // Arrange
            var product = new ProductInfos
            {
                Barcode = 1234567890,
                Name = "Test Product",
                Quantity = 1,
                Dlc = DateTime.Today.AddDays(7),
                AddedAt = DateTime.Today,
                UrlImage = "https://example.com/image.jpg"
            };

            // Act
            var errors = ValidationUtils.GetProductValidationErrors(product);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void GetProductValidationErrors_WithNullProduct_ShouldReturnError()
        {
            // Act
            var errors = ValidationUtils.GetProductValidationErrors(null);

            // Assert
            errors.Should().ContainSingle("Le produit ne peut pas être null");
        }

        [Fact]
        public void GetProductValidationErrors_WithInvalidBarcode_ShouldReturnError()
        {
            // Arrange
            var product = new ProductInfos
            {
                Barcode = 123, // Invalid
                Name = "Test Product",
                Quantity = 1,
                Dlc = DateTime.Today.AddDays(7),
                AddedAt = DateTime.Today
            };

            // Act
            var errors = ValidationUtils.GetProductValidationErrors(product);

            // Assert
            errors.Should().Contain("Le code-barres n'est pas valide");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void GetProductValidationErrors_WithInvalidName_ShouldReturnError(string name)
        {
            // Arrange
            var product = new ProductInfos
            {
                Barcode = 1234567890,
                Name = name,
                Quantity = 1,
                Dlc = DateTime.Today.AddDays(7),
                AddedAt = DateTime.Today
            };

            // Act
            var errors = ValidationUtils.GetProductValidationErrors(product);

            // Assert
            errors.Should().Contain("Le nom du produit est obligatoire");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1000)]
        public void GetProductValidationErrors_WithInvalidQuantity_ShouldReturnError(int quantity)
        {
            // Arrange
            var product = new ProductInfos
            {
                Barcode = 1234567890,
                Name = "Test Product",
                Quantity = quantity,
                Dlc = DateTime.Today.AddDays(7),
                AddedAt = DateTime.Today
            };

            // Act
            var errors = ValidationUtils.GetProductValidationErrors(product);

            // Assert
            errors.Should().Contain("La quantité doit être comprise entre 1 et 999");
        }

        [Fact]
        public void GetProductValidationErrors_WithInvalidDlc_ShouldReturnError()
        {
            // Arrange
            var product = new ProductInfos
            {
                Barcode = 1234567890,
                Name = "Test Product",
                Quantity = 1,
                Dlc = DateTime.Today.AddDays(-1), // Yesterday
                AddedAt = DateTime.Today
            };

            // Act
            var errors = ValidationUtils.GetProductValidationErrors(product);

            // Assert
            errors.Should().Contain("La date de péremption n'est pas valide");
        }

        [Fact]
        public void GetProductValidationErrors_WithInvalidUrlImage_ShouldReturnError()
        {
            // Arrange
            var product = new ProductInfos
            {
                Barcode = 1234567890,
                Name = "Test Product",
                Quantity = 1,
                Dlc = DateTime.Today.AddDays(7),
                AddedAt = DateTime.Today,
                UrlImage = "invalid-url"
            };

            // Act
            var errors = ValidationUtils.GetProductValidationErrors(product);

            // Assert
            errors.Should().Contain("L'URL de l'image n'est pas valide");
        }

        [Fact]
        public void GetProductValidationErrors_WithFutureAddedAt_ShouldReturnError()
        {
            // Arrange
            var product = new ProductInfos
            {
                Barcode = 1234567890,
                Name = "Test Product",
                Quantity = 1,
                Dlc = DateTime.Today.AddDays(7),
                AddedAt = DateTime.Now.AddDays(1) // Tomorrow
            };

            // Act
            var errors = ValidationUtils.GetProductValidationErrors(product);

            // Assert
            errors.Should().Contain("La date d'ajout ne peut pas être dans le futur");
        }

        [Fact]
        public void GetProductValidationErrors_WithMultipleErrors_ShouldReturnAllErrors()
        {
            // Arrange
            var product = new ProductInfos
            {
                Barcode = 123, // Invalid
                Name = "", // Invalid
                Quantity = 0, // Invalid
                Dlc = DateTime.Today.AddDays(-1), // Invalid
                AddedAt = DateTime.Now.AddDays(1), // Invalid
                UrlImage = "invalid-url" // Invalid
            };

            // Act
            var errors = ValidationUtils.GetProductValidationErrors(product);

            // Assert
            errors.Should().HaveCount(6);
            errors.Should().Contain("Le code-barres n'est pas valide");
            errors.Should().Contain("Le nom du produit est obligatoire");
            errors.Should().Contain("La quantité doit être comprise entre 1 et 999");
            errors.Should().Contain("La date de péremption n'est pas valide");
            errors.Should().Contain("La date d'ajout ne peut pas être dans le futur");
            errors.Should().Contain("L'URL de l'image n'est pas valide");
        }

        [Fact]
        public void IsValidCategory_ShouldIncludeAllExpectedCategories()
        {
            // Arrange
            var expectedCategories = new[]
            {
                "Fruits", "Légumes", "Viandes", "Poissons", "Produits laitiers",
                "Céréales", "Boissons", "Conserves", "Surgelés", "Épices",
                "Condiments", "Boulangerie", "Pâtisserie", "Autre"
            };

            // Act & Assert
            foreach (var category in expectedCategories)
            {
                ValidationUtils.IsValidCategory(category).Should().BeTrue($"Category '{category}' should be valid");
                ValidationUtils.IsValidCategory(category.ToLower()).Should().BeTrue($"Category '{category.ToLower()}' should be valid (case insensitive)");
                ValidationUtils.IsValidCategory(category.ToUpper()).Should().BeTrue($"Category '{category.ToUpper()}' should be valid (case insensitive)");
            }
        }
    }
}
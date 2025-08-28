using System;
using Xunit;
using FluentAssertions;
using PerimApp.Core.Models;

namespace PerimApp.Tests.Models
{
    public class ProductInfosTests
    {
        [Fact]
        public void DaysRemaining_WhenDlcIsToday_ShouldReturnZero()
        {
            // Arrange
            var product = new ProductInfos
            {
                Dlc = DateTime.Today
            };

            // Act
            var result = product.DaysRemaining;

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void DaysRemaining_WhenDlcIsTomorrow_ShouldReturnOne()
        {
            // Arrange
            var product = new ProductInfos
            {
                Dlc = DateTime.Today.AddDays(1)
            };

            // Act
            var result = product.DaysRemaining;

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public void DaysRemaining_WhenDlcIsYesterday_ShouldReturnMinusOne()
        {
            // Arrange
            var product = new ProductInfos
            {
                Dlc = DateTime.Today.AddDays(-1)
            };

            // Act
            var result = product.DaysRemaining;

            // Assert
            result.Should().Be(-1);
        }

        [Theory]
        [InlineData(-1, "Exp.")]
        [InlineData(0, "Auj.")]
        [InlineData(1, "1j")]
        [InlineData(5, "5j")]
        [InlineData(30, "30j")]
        public void DaysRemainingTextMainPage_ShouldReturnCorrectFormat(int daysToAdd, string expected)
        {
            // Arrange
            var product = new ProductInfos
            {
                Dlc = DateTime.Today.AddDays(daysToAdd)
            };

            // Act
            var result = product.DaysRemainingTextMainPage;

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(-1, "Expiré")]
        [InlineData(0, "Aujourd'hui")]
        [InlineData(1, "1 jour")]
        [InlineData(5, "5 jours")]
        [InlineData(30, "30 jours")]
        public void DaysRemainingTextDetailsPage_ShouldReturnCorrectFormat(int daysToAdd, string expected)
        {
            // Arrange
            var product = new ProductInfos
            {
                Dlc = DateTime.Today.AddDays(daysToAdd)
            };

            // Act
            var result = product.DaysRemainingTextDetailsPage;

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(1000, 20)]
        [InlineData(9999, 20)]
        [InlineData(999, 24)]
        [InlineData(10000, 24)]
        public void DaysRemainingFontSize_ShouldReturnCorrectSize(int daysToAdd, double expectedSize)
        {
            // Arrange
            var product = new ProductInfos
            {
                Dlc = DateTime.Today.AddDays(daysToAdd)
            };

            // Act
            var result = product.DaysRemainingFontSize;

            // Assert
            result.Should().Be(expectedSize);
        }

        [Theory]
        [InlineData(-1, true)]
        [InlineData(0, false)]
        [InlineData(1, false)]
        public void IsExpired_ShouldReturnCorrectValue(int daysToAdd, bool expected)
        {
            // Arrange
            var product = new ProductInfos
            {
                Dlc = DateTime.Today.AddDays(daysToAdd)
            };

            // Act
            var result = product.IsExpired;

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, true)]
        [InlineData(1, false)]
        public void ExpiresNow_ShouldReturnCorrectValue(int daysToAdd, bool expected)
        {
            // Arrange
            var product = new ProductInfos
            {
                Dlc = DateTime.Today.AddDays(daysToAdd)
            };

            // Act
            var result = product.ExpiresNow;

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(-1, false)]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, true)]
        [InlineData(4, false)]
        public void ExpiresSoon_ShouldReturnCorrectValue(int daysToAdd, bool expected)
        {
            // Arrange
            var product = new ProductInfos
            {
                Dlc = DateTime.Today.AddDays(daysToAdd)
            };

            // Act
            var result = product.ExpiresSoon;

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void IsValid_WhenAllFieldsAreValid_ShouldReturnTrue()
        {
            // Arrange
            var product = new ProductInfos
            {
                Barcode = 1234567890,
                Name = "Test Product",
                Quantity = 1,
                Dlc = DateTime.Today.AddDays(7),
                AddedAt = DateTime.Today
            };

            // Act
            var result = product.IsValid();

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(0, "Test Product", 1)] // Barcode invalid
        [InlineData(1234567890, "", 1)] // Name empty
        [InlineData(1234567890, "Test Product", 0)] // Quantity invalid
        public void IsValid_WhenFieldsAreInvalid_ShouldReturnFalse(long barcode, string name, int quantity)
        {
            // Arrange
            var product = new ProductInfos
            {
                Barcode = barcode,
                Name = name,
                Quantity = quantity,
                Dlc = DateTime.Today.AddDays(7),
                AddedAt = DateTime.Today
            };

            // Act
            var result = product.IsValid();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Clone_ShouldCreateExactCopy()
        {
            // Arrange
            var original = new ProductInfos
            {
                Id = 1,
                Barcode = 1234567890,
                Name = "Test Product",
                UrlImage = "http://example.com/image.jpg",
                Category = "Test Category",
                Conservation = "Frais",
                AddedAt = DateTime.Today,
                Dlc = DateTime.Today.AddDays(7),
                Quantity = 2,
                ProductUniqueId = "unique-id"
            };

            // Act
            var clone = original.Clone();

            // Assert
            clone.Should().NotBeSameAs(original);
            clone.Id.Should().Be(original.Id);
            clone.Barcode.Should().Be(original.Barcode);
            clone.Name.Should().Be(original.Name);
            clone.UrlImage.Should().Be(original.UrlImage);
            clone.Category.Should().Be(original.Category);
            clone.Conservation.Should().Be(original.Conservation);
            clone.AddedAt.Should().Be(original.AddedAt);
            clone.Dlc.Should().Be(original.Dlc);
            clone.Quantity.Should().Be(original.Quantity);
            clone.ProductUniqueId.Should().Be(original.ProductUniqueId);
        }

        [Fact]
        public void ProductUniqueId_ShouldHaveDefaultValue()
        {
            // Arrange & Act
            var product = new ProductInfos();

            // Assert
            product.ProductUniqueId.Should().NotBeNullOrEmpty();
            Guid.TryParse(product.ProductUniqueId, out _).Should().BeTrue();
        }

        [Fact]
        public void DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var product = new ProductInfos();

            // Assert
            product.Name.Should().Be(string.Empty);
            product.UrlImage.Should().Be(string.Empty);
            product.Category.Should().Be(string.Empty);
            product.Conservation.Should().Be(string.Empty);
            product.Id.Should().Be(0);
            product.Barcode.Should().Be(0);
            product.Quantity.Should().Be(0);
        }
    }
}
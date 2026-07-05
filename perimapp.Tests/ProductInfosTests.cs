using System;
using perimapp.Models;
using Xunit;

namespace perimapp.Tests
{
    public class ProductInfosTests
    {
        [Fact]
        public void ProductInfos_DisplayName_ShouldReturnCustomNameIfNotEmpty()
        {
            // Arrange
            var product = new ProductInfos
            {
                Name = "Original Name",
                CustomName = "Custom Name"
            };

            // Act & Assert
            Assert.Equal("Custom Name", product.DisplayName);
        }

        [Fact]
        public void ProductInfos_DisplayName_ShouldReturnOriginalNameIfCustomNameIsEmpty()
        {
            // Arrange
            var product = new ProductInfos
            {
                Name = "Original Name",
                CustomName = "  "
            };

            // Act & Assert
            Assert.Equal("Original Name", product.DisplayName);
        }

        [Fact]
        public void ProductInfos_DaysRemaining_ShouldBeZero_WhenDlcIsNull()
        {
            // Arrange
            var product = new ProductInfos
            {
                Name = "Test",
                Dlc = null
            };

            // Act & Assert
            Assert.Equal(0, product.DaysRemaining);
        }

        [Fact]
        public void ProductInfos_DaysRemaining_ShouldCalculateCorrectly_WhenDlcIsSet()
        {
            // Arrange
            var today = DateTime.Today;
            var targetDlc = today.AddDays(5);
            var product = new ProductInfos
            {
                Name = "Test",
                Dlc = new DateOnly(targetDlc.Year, targetDlc.Month, targetDlc.Day)
            };

            // Act & Assert
            Assert.Equal(5, product.DaysRemaining);
        }

        [Theory]
        [InlineData(-1, "Exp.")]
        [InlineData(0, "Auj.")]
        [InlineData(1, "1j")]
        [InlineData(5, "5j")]
        public void ProductInfos_DaysRemainingTextMainView_ShouldReturnExpectedText(int offset, string expectedText)
        {
            // Arrange
            var dlc = DateTime.Today.AddDays(offset);
            var product = new ProductInfos
            {
                Name = "Test",
                Dlc = new DateOnly(dlc.Year, dlc.Month, dlc.Day)
            };

            // Act & Assert
            Assert.Equal(expectedText, product.DaysRemainingTextMainView);
        }

        [Theory]
        [InlineData(-1, "Expiré")]
        [InlineData(0, "Aujourd'hui")]
        [InlineData(1, "1 jour")]
        [InlineData(5, "5 jours")]
        public void ProductInfos_DaysRemainingTextDetailsView_ShouldReturnExpectedText(int offset, string expectedText)
        {
            // Arrange
            var dlc = DateTime.Today.AddDays(offset);
            var product = new ProductInfos
            {
                Name = "Test",
                Dlc = new DateOnly(dlc.Year, dlc.Month, dlc.Day)
            };

            // Act & Assert
            Assert.Equal(expectedText, product.DaysRemainingTextDetailsView);
        }

        [Theory]
        [InlineData(50, 24)]
        [InlineData(1500, 20)]
        [InlineData(10000, 24)]
        public void ProductInfos_DaysRemainingFontSize_ShouldReturnExpectedSize(int offset, double expectedSize)
        {
            // Arrange
            var dlc = DateTime.Today.AddDays(offset);
            var product = new ProductInfos
            {
                Name = "Test",
                Dlc = new DateOnly(dlc.Year, dlc.Month, dlc.Day)
            };

            // Act & Assert
            Assert.Equal(expectedSize, product.DaysRemainingFontSize);
        }
    }
}

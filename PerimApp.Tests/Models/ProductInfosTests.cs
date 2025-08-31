using System;
using Xunit;

namespace PerimApp.Tests.Models
{
    // Test version of ProductInfos model to avoid MAUI dependencies
    public class ProductInfosTestModel
    {
        public int Id { get; set; }
        public long Barcode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UrlImage { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Conservation { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
        public DateTime Dlc { get; set; }
        public int DaysRemaining => (Dlc - DateTime.Today).Days;
        public int Quantity { get; set; }

        public string DaysRemainingTextMainPage
        {
            get
            {
                int days = DaysRemaining;
                if (days < 0)
                    return "Exp.";
                if (days == 0)
                    return "Auj.";
                if (days == 1)
                    return "1j";
                return $"{days}j";
            }
        }

        public double DaysRemainingFontSize
        {
            get
            {
                if (DaysRemaining >= 1000 && DaysRemaining <= 9999)
                {
                    return 20; // 4 chiffres, on réduit la taille
                }
                return 24; // Taille de police par défaut
            }
        }

        public string DaysRemainingTextDetailsPage
        {
            get
            {
                int days = DaysRemaining;
                if (days < 0)
                    return "Expiré";
                if (days == 0)
                    return "Aujourd'hui";
                if (days == 1)
                    return "1 jour";
                return $"{days} jours";
            }
        }

        public string ProductUniqueId { get; set; } = Guid.NewGuid().ToString();
    }

    public class ProductInfosTests
    {
        [Fact]
        public void DaysRemaining_ShouldCalculateCorrectly_WhenDlcIsInFuture()
        {
            // Arrange
            var product = new ProductInfosTestModel
            {
                Dlc = DateTime.Today.AddDays(5)
            };

            // Act
            var result = product.DaysRemaining;

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void DaysRemaining_ShouldCalculateCorrectly_WhenDlcIsToday()
        {
            // Arrange
            var product = new ProductInfosTestModel
            {
                Dlc = DateTime.Today
            };

            // Act
            var result = product.DaysRemaining;

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void DaysRemaining_ShouldCalculateCorrectly_WhenDlcIsInPast()
        {
            // Arrange
            var product = new ProductInfosTestModel
            {
                Dlc = DateTime.Today.AddDays(-3)
            };

            // Act
            var result = product.DaysRemaining;

            // Assert
            Assert.Equal(-3, result);
        }

        [Theory]
        [InlineData(-5, "Exp.")]
        [InlineData(-1, "Exp.")]
        [InlineData(0, "Auj.")]
        [InlineData(1, "1j")]
        [InlineData(5, "5j")]
        [InlineData(10, "10j")]
        public void DaysRemainingTextMainPage_ShouldReturnCorrectFormat(int daysOffset, string expected)
        {
            // Arrange
            var product = new ProductInfosTestModel
            {
                Dlc = DateTime.Today.AddDays(daysOffset)
            };

            // Act
            var result = product.DaysRemainingTextMainPage;

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(-5, "Expiré")]
        [InlineData(-1, "Expiré")]
        [InlineData(0, "Aujourd'hui")]
        [InlineData(1, "1 jour")]
        [InlineData(5, "5 jours")]
        [InlineData(10, "10 jours")]
        public void DaysRemainingTextDetailsPage_ShouldReturnCorrectFormat(int daysOffset, string expected)
        {
            // Arrange
            var product = new ProductInfosTestModel
            {
                Dlc = DateTime.Today.AddDays(daysOffset)
            };

            // Act
            var result = product.DaysRemainingTextDetailsPage;

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(999, 24)]
        [InlineData(1000, 20)]
        [InlineData(5000, 20)]
        [InlineData(9999, 20)]
        [InlineData(10000, 24)]
        public void DaysRemainingFontSize_ShouldReturnCorrectSize(int daysOffset, double expected)
        {
            // Arrange
            var product = new ProductInfosTestModel
            {
                Dlc = DateTime.Today.AddDays(daysOffset)
            };

            // Act
            var result = product.DaysRemainingFontSize;

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ProductUniqueId_ShouldBeGenerated()
        {
            // Arrange & Act
            var product = new ProductInfosTestModel();

            // Assert
            Assert.NotNull(product.ProductUniqueId);
            Assert.NotEmpty(product.ProductUniqueId);
        }

        [Fact]
        public void ProductUniqueId_ShouldBeUnique()
        {
            // Arrange & Act
            var product1 = new ProductInfosTestModel();
            var product2 = new ProductInfosTestModel();

            // Assert
            Assert.NotEqual(product1.ProductUniqueId, product2.ProductUniqueId);
        }
    }
}
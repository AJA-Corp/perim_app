using System;
using Xunit;
using FluentAssertions;
using PerimApp.Core.Converters;
using PerimApp.Core.Models;

namespace PerimApp.Tests.Converters
{
    public class DlcColorConverterTests
    {
        [Theory]
        [InlineData(-5, DlcColorConverter.ColorResult.Red)]
        [InlineData(-1, DlcColorConverter.ColorResult.Red)]
        [InlineData(0, DlcColorConverter.ColorResult.Orange)]
        [InlineData(1, DlcColorConverter.ColorResult.Orange)]
        [InlineData(2, DlcColorConverter.ColorResult.Yellow)]
        [InlineData(3, DlcColorConverter.ColorResult.Yellow)]
        [InlineData(4, DlcColorConverter.ColorResult.Green)]
        [InlineData(10, DlcColorConverter.ColorResult.Green)]
        [InlineData(365, DlcColorConverter.ColorResult.Green)]
        public void GetColorFromDays_ShouldReturnCorrectColor(int daysRemaining, DlcColorConverter.ColorResult expected)
        {
            // Act
            var result = DlcColorConverter.GetColorFromDays(daysRemaining);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void GetColorFromProduct_WithNullProduct_ShouldReturnGreen()
        {
            // Act
            var result = DlcColorConverter.GetColorFromProduct(null);

            // Assert
            result.Should().Be(DlcColorConverter.ColorResult.Green);
        }

        [Theory]
        [InlineData(-1, DlcColorConverter.ColorResult.Red)]
        [InlineData(0, DlcColorConverter.ColorResult.Orange)]
        [InlineData(1, DlcColorConverter.ColorResult.Orange)]
        [InlineData(2, DlcColorConverter.ColorResult.Yellow)]
        [InlineData(3, DlcColorConverter.ColorResult.Yellow)]
        [InlineData(7, DlcColorConverter.ColorResult.Green)]
        public void GetColorFromProduct_ShouldReturnCorrectColor(int daysToAdd, DlcColorConverter.ColorResult expected)
        {
            // Arrange
            var product = new ProductInfos
            {
                Dlc = DateTime.Today.AddDays(daysToAdd)
            };

            // Act
            var result = DlcColorConverter.GetColorFromProduct(product);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(DlcColorConverter.ColorResult.Red, "#FF4444")]
        [InlineData(DlcColorConverter.ColorResult.Orange, "#FF8800")]
        [InlineData(DlcColorConverter.ColorResult.Yellow, "#FFBB33")]
        [InlineData(DlcColorConverter.ColorResult.Green, "#00C851")]
        public void GetHexColor_ShouldReturnCorrectHexCode(DlcColorConverter.ColorResult colorResult, string expectedHex)
        {
            // Act
            var result = DlcColorConverter.GetHexColor(colorResult);

            // Assert
            result.Should().Be(expectedHex);
        }

        [Theory]
        [InlineData(DlcColorConverter.ColorResult.Red, true)]
        [InlineData(DlcColorConverter.ColorResult.Orange, true)]
        [InlineData(DlcColorConverter.ColorResult.Yellow, false)]
        [InlineData(DlcColorConverter.ColorResult.Green, false)]
        public void IsDangerous_ShouldReturnCorrectValue(DlcColorConverter.ColorResult colorResult, bool expected)
        {
            // Act
            var result = DlcColorConverter.IsDangerous(colorResult);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(-5, "Produit expiré")]
        [InlineData(-1, "Produit expiré")]
        [InlineData(0, "Expire aujourd'hui")]
        [InlineData(1, "Expire demain")]
        [InlineData(2, "Expire après-demain")]
        [InlineData(3, "Expire dans 3 jours")]
        [InlineData(4, "Expire cette semaine")]
        [InlineData(7, "Expire cette semaine")]
        [InlineData(8, "Expire ce mois")]
        [InlineData(15, "Expire ce mois")]
        [InlineData(30, "Expire ce mois")]
        [InlineData(31, "Expire plus tard")]
        [InlineData(365, "Expire plus tard")]
        public void GetUrgencyDescription_ShouldReturnCorrectDescription(int daysRemaining, string expected)
        {
            // Act
            var result = DlcColorConverter.GetUrgencyDescription(daysRemaining);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void GetColorFromDays_BoundaryValues_ShouldBeHandledCorrectly()
        {
            // Test boundary values specifically
            DlcColorConverter.GetColorFromDays(-1).Should().Be(DlcColorConverter.ColorResult.Red);
            DlcColorConverter.GetColorFromDays(0).Should().Be(DlcColorConverter.ColorResult.Orange);
            DlcColorConverter.GetColorFromDays(1).Should().Be(DlcColorConverter.ColorResult.Orange);
            DlcColorConverter.GetColorFromDays(2).Should().Be(DlcColorConverter.ColorResult.Yellow);
            DlcColorConverter.GetColorFromDays(3).Should().Be(DlcColorConverter.ColorResult.Yellow);
            DlcColorConverter.GetColorFromDays(4).Should().Be(DlcColorConverter.ColorResult.Green);
        }

        [Fact]
        public void AllColorResults_ShouldHaveValidHexCodes()
        {
            // Arrange
            var allColors = Enum.GetValues<DlcColorConverter.ColorResult>();

            // Act & Assert
            foreach (var color in allColors)
            {
                var hex = DlcColorConverter.GetHexColor(color);
                
                hex.Should().NotBeNullOrEmpty();
                hex.Should().StartWith("#");
                hex.Length.Should().Be(7); // #RRGGBB format
                
                // Verify it's a valid hex color (after #)
                var hexPart = hex[1..];
                foreach (var c in hexPart)
                {
                    "0123456789ABCDEFabcdef".Should().Contain(c.ToString());
                }
            }
        }

        [Fact]
        public void UrgencyDescriptions_ShouldCoverAllRanges()
        {
            // Test that we have descriptions for various ranges
            var testValues = new[] { -10, -1, 0, 1, 2, 3, 5, 7, 10, 20, 30, 50, 100, 365 };
            
            foreach (var value in testValues)
            {
                var description = DlcColorConverter.GetUrgencyDescription(value);
                description.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public void Color_Logic_ShouldBeConsistent()
        {
            // Red: expired products (dangerous)
            DlcColorConverter.IsDangerous(DlcColorConverter.ColorResult.Red).Should().BeTrue();
            
            // Orange: expires today or tomorrow (dangerous)
            DlcColorConverter.IsDangerous(DlcColorConverter.ColorResult.Orange).Should().BeTrue();
            
            // Yellow: expires in 2-3 days (warning, not dangerous)
            DlcColorConverter.IsDangerous(DlcColorConverter.ColorResult.Yellow).Should().BeFalse();
            
            // Green: expires later (safe)
            DlcColorConverter.IsDangerous(DlcColorConverter.ColorResult.Green).Should().BeFalse();
        }

        [Fact]
        public void ExtremeCases_ShouldBeHandledGracefully()
        {
            // Test extreme values
            DlcColorConverter.GetColorFromDays(int.MinValue).Should().Be(DlcColorConverter.ColorResult.Red);
            DlcColorConverter.GetColorFromDays(int.MaxValue).Should().Be(DlcColorConverter.ColorResult.Green);
            
            DlcColorConverter.GetUrgencyDescription(int.MinValue).Should().Be("Produit expiré");
            DlcColorConverter.GetUrgencyDescription(int.MaxValue).Should().Be("Expire plus tard");
        }
    }
}
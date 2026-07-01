using System;
using System.Globalization;
using Microsoft.Maui.Graphics;
using perimapp.Converters;
using Xunit;

namespace perimapp.Tests
{
    public class ConvertersTests
    {
        [Fact]
        public void DlcColorConverter_ShouldReturnGray_WhenValueIsNotInt()
        {
            // Arrange
            var converter = new DlcColorConverter();

            // Act
            var result = converter.Convert("not an int", typeof(Color), null, CultureInfo.InvariantCulture);

            // Assert
            var color = Assert.IsType<Color>(result);
            Assert.Equal("#808080", color.ToHex());
        }

        [Theory]
        [InlineData(-5, "#696969")]   // < 0
        [InlineData(0, "#FF0000")]    // <= 1 (specifically <= 0/1)
        [InlineData(1, "#FF0000")]    // <= 1
        [InlineData(2, "#F94144")]    // <= 2
        [InlineData(3, "#F8961E")]    // <= 3
        [InlineData(4, "#F9C74F")]    // <= 5
        [InlineData(5, "#F9C74F")]    // <= 5
        [InlineData(6, "#8CD6BF")]    // <= 7
        [InlineData(7, "#8CD6BF")]    // <= 7
        [InlineData(10, "#30C2FF")]   // > 7
        public void DlcColorConverter_ShouldReturnCorrectColor_ForDaysRemaining(int days, string expectedHex)
        {
            // Arrange
            var converter = new DlcColorConverter();

            // Act
            var result = converter.Convert(days, typeof(Color), null, CultureInfo.InvariantCulture);

            // Assert
            var color = Assert.IsType<Color>(result);
            Assert.Equal(expectedHex, color.ToHex());
        }

        [Fact]
        public void DlcColorConverter_ConvertBack_ShouldThrowNotImplementedException()
        {
            // Arrange
            var converter = new DlcColorConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(null, typeof(int), null, CultureInfo.InvariantCulture));
        }
    }
}

using System;
using Xunit;
using FluentAssertions;
using PerimApp.Core.Utilities;

namespace PerimApp.Tests.Utilities
{
    public class DateUtilsTests
    {
        [Theory]
        [InlineData("2024-01-01", "2024-01-01", 0)]
        [InlineData("2024-01-01", "2024-01-02", 1)]
        [InlineData("2024-01-01", "2024-01-10", 9)]
        [InlineData("2024-01-10", "2024-01-01", -9)]
        [InlineData("2024-02-28", "2024-03-01", 2)] // Leap year
        [InlineData("2023-02-28", "2023-03-01", 1)] // Non-leap year
        public void DaysBetween_ShouldCalculateCorrectly(string startDateStr, string endDateStr, int expected)
        {
            // Arrange
            var startDate = DateTime.Parse(startDateStr);
            var endDate = DateTime.Parse(endDateStr);

            // Act
            var result = DateUtils.DaysBetween(startDate, endDate);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void IsInPast_WithPastDate_ShouldReturnTrue()
        {
            // Arrange
            var pastDate = DateTime.Today.AddDays(-1);

            // Act
            var result = DateUtils.IsInPast(pastDate);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsInPast_WithToday_ShouldReturnFalse()
        {
            // Arrange
            var today = DateTime.Today;

            // Act
            var result = DateUtils.IsInPast(today);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsInPast_WithFutureDate_ShouldReturnFalse()
        {
            // Arrange
            var futureDate = DateTime.Today.AddDays(1);

            // Act
            var result = DateUtils.IsInPast(futureDate);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsToday_WithToday_ShouldReturnTrue()
        {
            // Arrange
            var today = DateTime.Today;

            // Act
            var result = DateUtils.IsToday(today);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsToday_WithTodayButDifferentTime_ShouldReturnTrue()
        {
            // Arrange
            var todayWithTime = DateTime.Today.AddHours(15).AddMinutes(30);

            // Act
            var result = DateUtils.IsToday(todayWithTime);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsToday_WithYesterday_ShouldReturnFalse()
        {
            // Arrange
            var yesterday = DateTime.Today.AddDays(-1);

            // Act
            var result = DateUtils.IsToday(yesterday);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsToday_WithTomorrow_ShouldReturnFalse()
        {
            // Arrange
            var tomorrow = DateTime.Today.AddDays(1);

            // Act
            var result = DateUtils.IsToday(tomorrow);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(1, 7, true)]
        [InlineData(3, 7, true)]
        [InlineData(7, 7, true)]
        [InlineData(8, 7, false)]
        [InlineData(0, 7, false)] // Today
        [InlineData(-1, 7, false)] // Yesterday
        public void IsInNearFuture_ShouldReturnCorrectValue(int daysToAdd, int threshold, bool expected)
        {
            // Arrange
            var date = DateTime.Today.AddDays(daysToAdd);

            // Act
            var result = DateUtils.IsInNearFuture(date, threshold);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void IsInNearFuture_WithDefaultThreshold_ShouldUseSevenDays()
        {
            // Arrange
            var dateInSixDays = DateTime.Today.AddDays(6);
            var dateInEightDays = DateTime.Today.AddDays(8);

            // Act
            var result1 = DateUtils.IsInNearFuture(dateInSixDays);
            var result2 = DateUtils.IsInNearFuture(dateInEightDays);

            // Assert
            result1.Should().BeTrue();
            result2.Should().BeFalse();
        }

        [Theory]
        [InlineData("2024-01-15", "15/01/2024")]
        [InlineData("2024-12-31", "31/12/2024")]
        [InlineData("2024-02-29", "29/02/2024")] // Leap year
        public void FormatFrenchDate_ShouldFormatCorrectly(string inputDateStr, string expected)
        {
            // Arrange
            var date = DateTime.Parse(inputDateStr);

            // Act
            var result = DateUtils.FormatFrenchDate(date);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("2024-01-15 14:30:45", "15/01/2024 14:30")]
        [InlineData("2024-12-31 23:59:59", "31/12/2024 23:59")]
        [InlineData("2024-02-29 09:05:30", "29/02/2024 09:05")]
        public void FormatFrenchDateTime_ShouldFormatCorrectly(string inputDateTimeStr, string expected)
        {
            // Arrange
            var dateTime = DateTime.Parse(inputDateTimeStr);

            // Act
            var result = DateUtils.FormatFrenchDateTime(dateTime);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void DaysBetween_WithSameDate_ShouldReturnZero()
        {
            // Arrange
            var date = new DateTime(2024, 6, 15);

            // Act
            var result = DateUtils.DaysBetween(date, date);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void DaysBetween_ShouldIgnoreTime()
        {
            // Arrange
            var date1 = new DateTime(2024, 6, 15, 10, 30, 0);
            var date2 = new DateTime(2024, 6, 15, 23, 45, 0);

            // Act
            var result = DateUtils.DaysBetween(date1, date2);

            // Assert
            result.Should().Be(0); // Same date, different times
        }

        [Fact]
        public void IsInNearFuture_WithEdgeCases_ShouldHandleCorrectly()
        {
            // Test edge cases for IsInNearFuture
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var dayAfterThreshold = today.AddDays(8); // Beyond default 7-day threshold

            // Today should not be considered "near future"
            DateUtils.IsInNearFuture(today).Should().BeFalse();

            // Tomorrow should be considered "near future"
            DateUtils.IsInNearFuture(tomorrow).Should().BeTrue();

            // Day after threshold should not be considered "near future"
            DateUtils.IsInNearFuture(dayAfterThreshold).Should().BeFalse();
        }

        [Fact]
        public void FormatMethods_WithMinAndMaxValues_ShouldNotThrow()
        {
            // Test that formatting methods don't throw with extreme values
            var action1 = () => DateUtils.FormatFrenchDate(DateTime.MinValue);
            var action2 = () => DateUtils.FormatFrenchDate(DateTime.MaxValue);
            var action3 = () => DateUtils.FormatFrenchDateTime(DateTime.MinValue);
            var action4 = () => DateUtils.FormatFrenchDateTime(DateTime.MaxValue);

            action1.Should().NotThrow();
            action2.Should().NotThrow();
            action3.Should().NotThrow();
            action4.Should().NotThrow();
        }

        [Fact]
        public void DaysBetween_WithLargeTimeSpans_ShouldCalculateCorrectly()
        {
            // Test with large time spans
            var date1 = new DateTime(2020, 1, 1);
            var date2 = new DateTime(2024, 1, 1);

            var result = DateUtils.DaysBetween(date1, date2);

            // 2020-2024 includes one leap year (2020)
            var expectedDays = (365 * 4) + 1; // 4 years plus 1 leap day
            result.Should().Be(expectedDays);
        }

        [Theory]
        [InlineData(0)] // Custom threshold of 0
        [InlineData(1)]
        [InlineData(30)]
        [InlineData(365)]
        public void IsInNearFuture_WithCustomThresholds_ShouldWorkCorrectly(int customThreshold)
        {
            // Arrange
            var dateWithinThreshold = DateTime.Today.AddDays(customThreshold);
            var dateBeyondThreshold = DateTime.Today.AddDays(customThreshold + 1);

            // Act
            var withinResult = DateUtils.IsInNearFuture(dateWithinThreshold, customThreshold);
            var beyondResult = DateUtils.IsInNearFuture(dateBeyondThreshold, customThreshold);

            // Assert
            if (customThreshold == 0)
            {
                withinResult.Should().BeFalse(); // Today is not considered "near future"
            }
            else
            {
                withinResult.Should().BeTrue();
            }
            beyondResult.Should().BeFalse();
        }
    }
}
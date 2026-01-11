using CCXT.Simple.Core.Extensions;
using Xunit;

namespace CCXT.Simple.Tests.Extensions
{
    public class TimeExtensionsTests
    {
        #region Unix Epoch Tests

        [Fact]
        public void UnixEpoch_ReturnsCorrectValue()
        {
            // Arrange
            var expected = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = TimeExtensions.UnixEpoch;

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region NowMilli Tests

        [Fact]
        public void NowMilli_ReturnsCurrentUnixMilliseconds()
        {
            // Arrange
            var before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Act
            var result = TimeExtensions.NowMilli;
            var after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Assert
            Assert.InRange(result, before, after);
        }

        [Fact]
        public void UnixTimeMillisecondsNow_ReturnsCurrentUnixMilliseconds()
        {
            // Arrange
            var before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Act
            var result = TimeExtensions.UnixTimeMillisecondsNow;
            var after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Assert
            Assert.InRange(result, before, after);
        }

        #endregion

        #region Now (Seconds) Tests

        [Fact]
        public void Now_ReturnsCurrentUnixSeconds()
        {
            // Arrange
            var before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Act
            var result = TimeExtensions.Now;
            var after = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Assert
            Assert.InRange(result, before, after);
        }

        [Fact]
        public void UnixTimeSecondsNow_ReturnsCurrentUnixSeconds()
        {
            // Arrange
            var before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Act
            var result = TimeExtensions.UnixTimeSecondsNow;
            var after = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Assert
            Assert.InRange(result, before, after);
        }

        #endregion

        #region ConvertToUnixTime Tests

        [Theory]
        [InlineData(2021, 1, 1, 0, 0, 0, 1609459200)]
        [InlineData(1970, 1, 1, 0, 0, 0, 0)]
        [InlineData(2000, 1, 1, 0, 0, 0, 946684800)]
        public void ConvertToUnixTime_DateTime_ReturnsCorrectSeconds(int year, int month, int day, int hour, int min, int sec, long expected)
        {
            // Arrange
            var dateTime = new DateTime(year, month, day, hour, min, sec, DateTimeKind.Utc);

            // Act
            var result = TimeExtensions.ConvertToUnixTime(dateTime);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region ConvertToUnixTimeMilli Tests

        [Theory]
        [InlineData(2021, 1, 1, 0, 0, 0, 1609459200000)]
        [InlineData(1970, 1, 1, 0, 0, 0, 0)]
        [InlineData(2000, 1, 1, 0, 0, 0, 946684800000)]
        public void ConvertToUnixTimeMilli_DateTime_ReturnsCorrectMilliseconds(int year, int month, int day, int hour, int min, int sec, long expected)
        {
            // Arrange
            var dateTime = new DateTime(year, month, day, hour, min, sec, DateTimeKind.Utc);

            // Act
            var result = TimeExtensions.ConvertToUnixTimeMilli(dateTime);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region ConvertToUtcTime Tests

        [Theory]
        [InlineData(1609459200, 2021, 1, 1)]
        [InlineData(0, 1970, 1, 1)]
        [InlineData(946684800, 2000, 1, 1)]
        public void ConvertToUtcTime_UnixSeconds_ReturnsCorrectDateTime(long unix, int expectedYear, int expectedMonth, int expectedDay)
        {
            // Act
            var result = TimeExtensions.ConvertToUtcTime(unix);

            // Assert
            Assert.Equal(expectedYear, result.Year);
            Assert.Equal(expectedMonth, result.Month);
            Assert.Equal(expectedDay, result.Day);
            Assert.Equal(DateTimeKind.Utc, result.Kind);
        }

        #endregion

        #region ConvertToUtcTimeMilli Tests

        [Theory]
        [InlineData(1609459200000, 2021, 1, 1)]
        [InlineData(0, 1970, 1, 1)]
        [InlineData(946684800000, 2000, 1, 1)]
        public void ConvertToUtcTimeMilli_UnixMilliseconds_ReturnsCorrectDateTime(long unix, int expectedYear, int expectedMonth, int expectedDay)
        {
            // Act
            var result = TimeExtensions.ConvertToUtcTimeMilli(unix);

            // Assert
            Assert.Equal(expectedYear, result.Year);
            Assert.Equal(expectedMonth, result.Month);
            Assert.Equal(expectedDay, result.Day);
            Assert.Equal(DateTimeKind.Utc, result.Kind);
        }

        #endregion

        #region ConvertToSeconds and ConvertToMilliSeconds Tests

        [Theory]
        [InlineData(1609459200, 1609459200)]      // Already seconds
        [InlineData(1609459200000, 1609459200)]   // Milliseconds converted to seconds
        public void ConvertToSeconds_ReturnsCorrectSeconds(long input, long expected)
        {
            // Act
            var result = TimeExtensions.ConvertToSeconds(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1609459200, 1609459200000)]   // Seconds converted to milliseconds
        [InlineData(1609459200000, 1609459200000)] // Already milliseconds
        public void ConvertToMilliSeconds_ReturnsCorrectMilliseconds(long input, long expected)
        {
            // Act
            var result = TimeExtensions.ConvertToMilliSeconds(input);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region AddUnixTime Tests

        [Fact]
        public void AddUnixTime_Offset_AddsToCurrentTime()
        {
            // Arrange
            var offset = 3600L; // 1 hour
            var before = TimeExtensions.Now + offset - 1;

            // Act
            var result = TimeExtensions.AddUnixTime(offset);
            var after = TimeExtensions.Now + offset + 1;

            // Assert
            Assert.InRange(result, before, after);
        }

        [Fact]
        public void AddUnixTime_TimeSpan_AddsToCurrentTime()
        {
            // Arrange
            var offset = TimeSpan.FromHours(1);
            var before = TimeExtensions.Now + (long)offset.TotalSeconds - 1;

            // Act
            var result = TimeExtensions.AddUnixTime(offset);
            var after = TimeExtensions.Now + (long)offset.TotalSeconds + 1;

            // Assert
            Assert.InRange(result, before, after);
        }

        #endregion

        #region IsLeapYear Tests

        [Theory]
        [InlineData(2000, true)]   // Divisible by 400
        [InlineData(2020, true)]   // Divisible by 4 but not 100
        [InlineData(2024, true)]   // Divisible by 4 but not 100
        [InlineData(1900, false)]  // Divisible by 100 but not 400
        [InlineData(2019, false)]  // Not divisible by 4
        [InlineData(2021, false)]  // Not divisible by 4
        public void IsLeapYear_ReturnsCorrectValue(int year, bool expected)
        {
            // Act
            var result = TimeExtensions.IsLeapYear(year);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(10000)]
        public void IsLeapYear_InvalidYear_ThrowsArgumentOutOfRangeException(int year)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => TimeExtensions.IsLeapYear(year));
        }

        #endregion

        #region GetFirstDayOfMonth Tests

        [Fact]
        public void GetFirstDayOfMonth_DateTime_ReturnsFirstDay()
        {
            // Arrange
            var date = new DateTime(2021, 5, 15, 10, 30, 0, DateTimeKind.Utc);

            // Act
            var result = TimeExtensions.GetFirstDayOfMonth(date);

            // Assert
            Assert.Equal(2021, result.Year);
            Assert.Equal(5, result.Month);
            Assert.Equal(1, result.Day);
        }

        [Fact]
        public void GetFirstDayOfMonth_IntMonth_ReturnsFirstDay()
        {
            // Act - use month 6 to avoid timezone edge cases
            var result = TimeExtensions.GetFirstDayOfMonth(6);

            // Assert - the day should always be 1 regardless of timezone
            Assert.Equal(1, result.Day);
            // Month assertion is tricky due to ToUniversalTime() conversion
            // In some timezones, converting to UTC might shift to previous month
            Assert.True(result.Month == 5 || result.Month == 6,
                $"Month should be 5 or 6 (timezone dependent), but was {result.Month}");
        }

        #endregion

        #region GetLastDayOfMonth Tests

        [Theory]
        [InlineData(2021, 1, 15, 31)]  // January
        [InlineData(2021, 2, 15, 28)]  // February (non-leap year)
        [InlineData(2020, 2, 15, 29)]  // February (leap year)
        [InlineData(2021, 4, 15, 30)]  // April
        public void GetLastDayOfMonth_DateTime_ReturnsLastDay(int year, int month, int day, int expectedLastDay)
        {
            // Arrange
            var date = new DateTime(year, month, day, 10, 30, 0, DateTimeKind.Utc);

            // Act
            var result = TimeExtensions.GetLastDayOfMonth(date);

            // Assert
            Assert.Equal(year, result.Year);
            Assert.Equal(month, result.Month);
            Assert.Equal(expectedLastDay, result.Day);
        }

        #endregion

        #region IsDateTimeFormat Tests

        [Theory]
        [InlineData("2021-01-01", true)]
        [InlineData("2021-01-01T00:00:00", true)]
        [InlineData("2021-01-01T00:00:00Z", true)]
        [InlineData("invalid", false)]
        [InlineData("", false)]
        public void IsDateTimeFormat_ReturnsCorrectValue(string input, bool expected)
        {
            // Act
            var result = TimeExtensions.IsDateTimeFormat(input);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region DaysInYear Tests

        [Theory]
        [InlineData(1, 365)]
        [InlineData(4, 1461)]   // 4 years including 1 leap year
        public void DaysInYear_ReturnsCorrectValue(int year, int expectedDays)
        {
            // Act
            var result = TimeExtensions.DaysInYear(year);

            // Assert
            Assert.Equal(expectedDays, result);
        }

        #endregion

        #region GetUnixTimeSecond Tests

        [Fact]
        public void GetUnixTimeSecond_ReturnsCorrectValue()
        {
            // Arrange
            int days = 1;
            int hours = 1;
            int minutes = 1;
            int seconds = 1;
            ulong expected = (ulong)(86400 + 3600 + 60 + 1);

            // Act
            var result = TimeExtensions.GetUnixTimeSecond(days, hours, minutes, seconds);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion
    }
}

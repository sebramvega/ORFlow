using ORFlow.Domain.Scheduling;

namespace ORFlow.Domain.Tests.Scheduling;

public class TimeRangeTests
{
    [Fact]
    public void Constructor_EndBeforeStart_ThrowsArgumentException()
    {
        // Arrange
        DateTimeOffset start = DateTimeOffset.UtcNow.AddHours(2);
        DateTimeOffset end = start.AddHours(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new TimeRange(start, end));
    }

    [Fact]
    public void Constructor_EndEqualsStart_ThrowsArgumentException()
    {
        // Arrange
        DateTimeOffset start = DateTimeOffset.UtcNow.AddHours(1);
        DateTimeOffset end = start;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new TimeRange(start, end));
    }

    [Fact]
    public void OverlapsWith_PartiallyOverlappingRanges_ReturnsTrue()
    {
        // Arrange
        DateTimeOffset start = DateTimeOffset.UtcNow;

        TimeRange first = new TimeRange(
            start,
            start.AddHours(2));

        TimeRange second = new TimeRange(
            start.AddHours(1),
            start.AddHours(3));

        // Act
        bool result = first.OverlapsWith(second);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OverlapsWith_ContainedRange_ReturnsTrue()
    {
        // Arrange
        DateTimeOffset start = DateTimeOffset.UtcNow;

        TimeRange outer = new TimeRange(
            start,
            start.AddHours(4));

        TimeRange inner = new TimeRange(
            start.AddHours(1),
            start.AddHours(2));

        // Act
        bool result = outer.OverlapsWith(inner);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OverlapsWith_SeparateRanges_ReturnsFalse()
    {
        // Arrange
        DateTimeOffset start = DateTimeOffset.UtcNow;

        TimeRange first = new TimeRange(
            start,
            start.AddHours(2));

        TimeRange second = new TimeRange(
            start.AddHours(3),
            start.AddHours(5));

        // Act
        bool result = first.OverlapsWith(second);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OverlapsWith_BackToBackRanges_ReturnsFalse()
    {
        // Arrange
        DateTimeOffset start = DateTimeOffset.UtcNow;

        TimeRange first = new TimeRange(
            start,
            start.AddHours(2));

        TimeRange second = new TimeRange(
            start.AddHours(2),
            start.AddHours(4));

        // Act
        bool result = first.OverlapsWith(second);

        // Assert
        Assert.False(result);
    }
}
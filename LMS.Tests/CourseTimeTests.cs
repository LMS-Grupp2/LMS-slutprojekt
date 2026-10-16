using LMS.Services;

namespace LMS.Tests;

/// <summary>
/// Swedish wall-clock time to UTC. Fixed dates, so the tests do not depend on the season they run in.
/// </summary>
public class CourseTimeTests
{
    [Fact]
    public void SummerTime_IsTwoHoursAheadOfUtc()
    {
        //ARRANGE
        var deadline = new DateTime(2026, 10, 20, 23, 59, 0);

        //ACT
        var utc = CourseTime.ToUtc(deadline);

        //ASSERT
        Assert.Equal(new DateTime(2026, 10, 20, 21, 59, 0, DateTimeKind.Utc), utc);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
    }

    [Fact]
    public void WinterTime_IsOneHourAheadOfUtc()
    {
        //ARRANGE: after the clocks go back on 25 Oct 2026
        var deadline = new DateTime(2026, 11, 20, 23, 59, 0);

        //ACT
        var utc = CourseTime.ToUtc(deadline);

        //ASSERT
        Assert.Equal(new DateTime(2026, 11, 20, 22, 59, 0, DateTimeKind.Utc), utc);
    }

    [Fact]
    public void Kind_IsIgnored_BecauseStoredActivityTimesAreAlwaysSwedish()
    {
        //ARRANGE: same wall-clock time, wrongly marked as UTC
        var deadline = new DateTime(2026, 11, 20, 23, 59, 0, DateTimeKind.Utc);

        //ACT
        var utc = CourseTime.ToUtc(deadline);

        //ASSERT
        Assert.Equal(new DateTime(2026, 11, 20, 22, 59, 0, DateTimeKind.Utc), utc);
    }

    [Fact]
    public void TimeSkippedWhenClocksGoForward_DoesNotThrow()
    {
        //ARRANGE: 02:30 on 28 Mar 2027 does not exist in Sweden (02:00 jumps to 03:00)
        var deadline = new DateTime(2027, 3, 28, 2, 30, 0);

        //ACT
        var utc = CourseTime.ToUtc(deadline);

        //ASSERT: treated as 03:30 summer time = 01:30 UTC
        Assert.Equal(new DateTime(2027, 3, 28, 1, 30, 0, DateTimeKind.Utc), utc);
    }
}

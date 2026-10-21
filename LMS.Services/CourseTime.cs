namespace LMS.Services;

/// <summary>
/// Activity times (for example the deadline, Activity.EndTime) are entered and stored as Swedish wall-clock time.
/// Timestamps set by the server (for example Submission.SubmittedAt) are UTC.
/// Convert with this helper before comparing the two. It names the time zone explicitly, so the result does not
/// depend on the server's own time zone (Azure App Service runs in UTC, a developer's machine in Swedish time).
/// </summary>
public static class CourseTime
{
    // IANA id. Works on Linux, macOS and Windows (.NET 6+).
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    /// <summary>Converts a Swedish wall-clock time, as stored on an activity, to UTC.</summary>
    public static DateTime ToUtc(DateTime courseTime)
    {
        // The stored value is wall-clock time whatever its Kind says, so drop the Kind before converting
        var local = DateTime.SpecifyKind(courseTime, DateTimeKind.Unspecified);

        // On the last Sunday in March the clock jumps from 02:00 to 03:00, so for example 02:30 does not exist.
        // Treat such a time as one hour later instead of letting the conversion throw.
        if (Zone.IsInvalidTime(local))
            local = local.AddHours(1);

        return TimeZoneInfo.ConvertTimeToUtc(local, Zone);
    }

    /// <summary>
    /// Converts a UTC time (for example DateTime.UtcNow) to Swedish wall-clock time, the time activities are
    /// stored in (US12). Use it to ask "has this activity ended?" or "what is today's date in Sweden?".
    /// </summary>
    public static DateTime FromUtc(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone);
}
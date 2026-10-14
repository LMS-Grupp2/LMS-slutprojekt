namespace LMS.Shared.Constants;

/// <summary>
/// Names of the seeded activity types that the code needs to recognise (see ApplicationDbContext.HasData).
/// Only "Assignment" has special rules today: it is the only type a student can hand in to.
/// </summary>
public static class ActivityTypeNames
{
    public const string Assignment = "Assignment";
}

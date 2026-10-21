namespace LMS.Shared.Constants;

public static class CourseStatus
{
    public const string Upcoming = "Upcoming";
    public const string Active = "Active";
    public const string Finished = "Finished";

    public static readonly IReadOnlyCollection<string> All = [Upcoming, Active, Finished];
}



namespace LMS.Shared.Constants;

public static class UserRoles
{
    public const string Teacher = "Teacher";
    public const string Student = "Student";

    public static readonly IReadOnlyCollection<string> Assignable = [Teacher, Student];
}


namespace LMS.Shared.Constants;

public static class ActivityTypes
{
    public const string Lecture = "Lecture";
    public const string ELearning = "E-Learning";
    public const string Exercise = "Exercise";
    public const string Assignment = "Assignment";
    public const string Other = "Other";

    public static readonly IReadOnlyCollection<string>
        ValidTypes = [Lecture, ELearning, Exercise, Assignment, Other];
}

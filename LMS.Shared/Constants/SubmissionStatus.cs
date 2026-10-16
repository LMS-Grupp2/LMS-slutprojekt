namespace LMS.Shared.Constants;

/// <summary>
/// The submission status names, shared by the API and Blazor so both use the exact same words.
/// The status is calculated from dates, never stored.
/// </summary>
public static class SubmissionStatus
{
    public const string NotSubmitted = "Not submitted";
    public const string AwaitingFeedback = "Awaiting feedback";
    public const string Late = "Late";
    public const string Missing = "Missing";
    public const string FeedbackGiven = "Feedback given";
}

using LMS.Shared.Constants;

namespace LMS.Blazor.Client.Helpers;

/// <summary>
/// The one place that decides how a submission status looks, so every page shows the same colours.
/// Tone names come from lms.css (lms-tone-*); text classes are Bootstrap.
/// </summary>
public static class SubmissionStatusStyles
{
    //Badge colour per status, for StatusBadge
    public static string ToneOf(string status) => status switch
    {
        SubmissionStatus.AwaitingFeedback => "success",
        SubmissionStatus.FeedbackGiven => "info",
        SubmissionStatus.Late => "warning",
        SubmissionStatus.Missing => "danger",
        SubmissionStatus.NotSubmitted => "muted",
        _ => "muted"
    };

    //Text colour for a number in that status's colour(meaning the status cards colour)
    public static string TextClassOf(string status) => status switch
    {
        SubmissionStatus.AwaitingFeedback => "text-success",
        SubmissionStatus.FeedbackGiven => "text-info-emphasis",
        SubmissionStatus.Late => "text-warning-emphasis",
        SubmissionStatus.Missing => "text-danger",
        SubmissionStatus.NotSubmitted => string.Empty,
        _ => string.Empty
    };
}

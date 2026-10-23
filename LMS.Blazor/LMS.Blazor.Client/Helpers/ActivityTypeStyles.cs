using LMS.Shared.Constants;

namespace LMS.Blazor.Client.Helpers;

/// <summary>
/// The one place that decides which colour an activity type gets, so every page shows the same colours.
/// Tone names come from lms.css (lms-tone-*), like SubmissionStatusStyles. Used by the dashboards.
/// </summary>
public static class ActivityTypeStyles
{
    // Badge colour per activity type, for StatusBadge. Unknown types get the neutral "muted" colour.
    public static string ToneOf(string typeName) => typeName switch
    {
        "Lecture" => "primary",
        "E-Learning" => "info",
        "Exercise" => "success",
        // The shared constant, so the name is defined in one place only
        ActivityTypeNames.Assignment => "warning",
        _ => "muted"
    };
}
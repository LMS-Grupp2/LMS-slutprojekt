using Domain.Models.Entities;
using LMS.Shared.Constants;

namespace LMS.Services;

/// <summary>
/// The one place that turns a submission (or the lack of one) into a status word from SubmissionStatus.
/// Pure: no database and no clock of its own. The caller passes "now", which makes the rule easy to test.
/// US08 (student's list), US09 (student's hand-in) and US10 (teacher's overview) should all call this.
/// </summary>
public static class SubmissionStatusCalculator
{
    // Status meanings:
    //     Not submitted:     nothing handed in, deadline not passed yet
    //     Missing:           nothing handed in, deadline has passed
    //     Awaiting feedback: handed in on time, waiting for the teacher
    //     Late:              handed in after the deadline, waiting for the teacher
    //     Feedback given:    the teacher has given feedback (wins over Late)

    /// <summary>
    /// Returns one of the SubmissionStatus constants.
    /// submission is null when the student has not handed in anything. When it is not null,
    /// its Feedbacks collection must be loaded (Include in the repository).
    /// deadline is Activity.EndTime and now is the current UTC time.
    /// </summary>
    public static string Calculate(Submission? submission, DateTime deadline, DateTime now)
    {
        if (submission is null)
            return now > deadline ? SubmissionStatus.Missing : SubmissionStatus.NotSubmitted;

        if (submission.Feedbacks.Any())
            return SubmissionStatus.FeedbackGiven;

        return IsLate(submission, deadline)
            ? SubmissionStatus.Late
            : SubmissionStatus.AwaitingFeedback;
    }

    /// <summary>
    /// True if the submission was handed in after the deadline. Strict >, so handing in exactly
    /// at the deadline is in time. Used both for the status word and for the IsLate flag in the DTO.
    /// </summary>
    public static bool IsLate(Submission submission, DateTime deadline) =>
        submission.SubmittedAt > deadline;

    /// <summary>
    /// True when the teacher has started assessing the submission (feedback or a grade exists, #60).
    /// A locked submission takes no new files, so the teacher never assesses something that changes afterwards.
    /// Feedbacks must be loaded (Include in the repository).
    /// </summary>
    public static bool IsLocked(Submission submission) =>
        submission.Feedbacks.Any() || submission.Grade.HasValue;
}

using Domain.Models.Entities;
using LMS.Services;
using LMS.Shared.Constants;

namespace LMS.Tests;

public class SubmissionStatusCalculatorTests
{
    // Fixed times keep the tests repeatable. DateTime.UtcNow would make them depend on the day they run.
    private static readonly DateTime Deadline = new(2026, 10, 20, 23, 59, 0, DateTimeKind.Utc);
    private static readonly DateTime BeforeDeadline = Deadline.AddDays(-1);
    private static readonly DateTime AfterDeadline = Deadline.AddHours(1);

    [Fact]
    public void NoSubmission_BeforeDeadline_IsNotSubmitted()
    {
        var status = SubmissionStatusCalculator.Calculate(null, Deadline, now: BeforeDeadline);

        Assert.Equal(SubmissionStatus.NotSubmitted, status);
    }

    [Fact]
    public void NoSubmission_AfterDeadline_IsMissing()
    {
        var status = SubmissionStatusCalculator.Calculate(null, Deadline, now: AfterDeadline);

        Assert.Equal(SubmissionStatus.Missing, status);
    }

    [Fact]
    public void SubmittedInTime_WithoutFeedback_IsAwaitingFeedback()
    {
        var submission = new Submission { SubmittedAt = BeforeDeadline };

        // "now" is after the deadline: once handed in, the status must not change because time passes
        var status = SubmissionStatusCalculator.Calculate(submission, Deadline, now: AfterDeadline);

        Assert.Equal(SubmissionStatus.AwaitingFeedback, status);
    }

    [Fact]
    public void SubmittedExactlyAtDeadline_IsNotLate()
    {
        var submission = new Submission { SubmittedAt = Deadline };

        var status = SubmissionStatusCalculator.Calculate(submission, Deadline, now: AfterDeadline);

        Assert.Equal(SubmissionStatus.AwaitingFeedback, status);
    }

    [Fact]
    public void SubmittedAfterDeadline_WithoutFeedback_IsLate()
    {
        var submission = new Submission { SubmittedAt = AfterDeadline };

        var status = SubmissionStatusCalculator.Calculate(submission, Deadline, now: AfterDeadline.AddDays(1));

        Assert.Equal(SubmissionStatus.Late, status);
    }

    [Fact]
    public void SubmittedInTime_WithFeedback_IsFeedbackGiven()
    {
        var submission = new Submission
        {
            SubmittedAt = BeforeDeadline,
            Feedbacks = [new Feedback { Text = "Good work" }]
        };

        var status = SubmissionStatusCalculator.Calculate(submission, Deadline, now: AfterDeadline);

        Assert.Equal(SubmissionStatus.FeedbackGiven, status);
    }

    [Fact]
    public void SubmittedLate_WithFeedback_IsFeedbackGiven()
    {
        var submission = new Submission
        {
            SubmittedAt = AfterDeadline,
            Feedbacks = [new Feedback { Text = "Late, but approved" }]
        };

        var status = SubmissionStatusCalculator.Calculate(submission, Deadline, now: AfterDeadline.AddDays(1));

        Assert.Equal(SubmissionStatus.FeedbackGiven, status);
    }
}

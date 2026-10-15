using Domain.Models.Entities;
using Domain.Models.Enums;
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
        //ACT
        var status = SubmissionStatusCalculator.Calculate(null, Deadline, now: BeforeDeadline);
        
        //ASSERT
        Assert.Equal(SubmissionStatus.NotSubmitted, status);
    }

    [Fact]
    public void NoSubmission_AfterDeadline_IsMissing()
    {   
        //ACT
        var status = SubmissionStatusCalculator.Calculate(null, Deadline, now: AfterDeadline);
        
        //ASSERT
        Assert.Equal(SubmissionStatus.Missing, status);
    }

    [Fact]
    public void SubmittedInTime_WithoutFeedback_IsAwaitingFeedback()
    {
        //ARRANGE
        var submission = new Submission { SubmittedAt = BeforeDeadline };

        //ACT
        var status = SubmissionStatusCalculator.Calculate(submission, Deadline, now: AfterDeadline);

        //ASSERT
        Assert.Equal(SubmissionStatus.AwaitingFeedback, status);
    }

    [Fact]
    public void SubmittedExactlyAtDeadline_IsNotLate()
    {
        //ARRANGE
        var submission = new Submission { SubmittedAt = Deadline };
        //ACT
        var status = SubmissionStatusCalculator.Calculate(submission, Deadline, now: AfterDeadline);

        //ASSERT
        Assert.Equal(SubmissionStatus.AwaitingFeedback, status);
    }

    [Fact]
    public void SubmittedAfterDeadline_WithoutFeedback_IsLate()
    {
        //ARRANGE
        var submission = new Submission { SubmittedAt = AfterDeadline };

        //ACT
        var status = SubmissionStatusCalculator.Calculate(submission, Deadline, now: AfterDeadline.AddDays(1));

        //ASSERT
        Assert.Equal(SubmissionStatus.Late, status);
    }

    [Fact]
    public void SubmittedInTime_WithFeedback_IsFeedbackGiven()
    {
        //ARRANGE
        var submission = new Submission
        {
            SubmittedAt = BeforeDeadline,
            Feedbacks = [new Feedback { Text = "Bra jobbat!" }]
        };

        //ACT
        var status = SubmissionStatusCalculator.Calculate(submission, Deadline, now: AfterDeadline);

        //ASSERT
        Assert.Equal(SubmissionStatus.FeedbackGiven, status);
    }

    [Fact]
    public void SubmittedLate_WithFeedback_IsFeedbackGiven()
    {   
        //ARRANGE
        var submission = new Submission
        {
            SubmittedAt = AfterDeadline,
            Feedbacks = [new Feedback { Text = "Godkänt + kompletteringsuppgift." }]
        };
        //ACT
        var status = SubmissionStatusCalculator.Calculate(submission, Deadline, now: AfterDeadline.AddDays(1));

        //ASSERT
        Assert.Equal(SubmissionStatus.FeedbackGiven, status);
    }

    [Fact]
    public void Submission_WithoutFeedbackOrGrade_IsNotLocked()
    {
        //ARRANGE
        var submission = new Submission { SubmittedAt = BeforeDeadline };

        //ACT + ASSERT
        Assert.False(SubmissionStatusCalculator.IsLocked(submission));
    }

    [Fact]
    public void Submission_WithFeedback_IsLocked()
    {
        //ARRANGE
        var submission = new Submission
        {
            SubmittedAt = BeforeDeadline,
            Feedbacks = [new Feedback { Text = "Good work" }]
        };

        //ACT + ASSERT
        Assert.True(SubmissionStatusCalculator.IsLocked(submission));
    }

    [Fact]
    public void Submission_WithGrade_IsLocked()
    {
        //ARRANGE
        var submission = new Submission { SubmittedAt = BeforeDeadline, Grade = Grade.C };

        //ACT + ASSERT
        Assert.True(SubmissionStatusCalculator.IsLocked(submission));
    }
}

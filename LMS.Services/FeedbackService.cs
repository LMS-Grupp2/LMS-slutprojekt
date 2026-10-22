using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.FeedbackDtos;
using Service.Contracts;

namespace LMS.Services;

public class FeedbackService(IUnitOfWork unitOfWork) : IFeedbackService
{
    public async Task<FeedbackDto> GiveFeedbackAsync(Guid submissionId, string teacherId, 
        CreateFeedbackDto dto)
    {
        var teacher = await unitOfWork.Users.FindByIdAsync(teacherId)
            ?? throw new UserNotFoundException(teacherId);

        var submission = await unitOfWork.Submissions.GetByIdAsync(submissionId)
            ?? throw new SubmissionNotFoundException(submissionId);

        var feedback = new Feedback
        {
            Id = Guid.NewGuid(),
            Text = dto.Text,
            CreatedAt = DateTime.UtcNow,
            SubmissionId = submission.Id,
            TeacherId = teacherId,
            Teacher = teacher
        };

        submission.Approved = dto.IsApproved!.Value;
        submission.NeedChanges = !dto.IsApproved.Value;


        unitOfWork.Feedbacks.Create(feedback);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(feedback);
    }

    /// <summary>
    /// All feedback on one submission, newest first. Teachers see every submission,
    /// a student only their own (US-1104). 404 if the submission doesn't exist or isn't theirs.
    /// </summary>
    public async Task<IEnumerable<FeedbackDto>> GetBySubmissionIdAsync(Guid submissionId,
       string userId, bool isTeacher)
    {
        var submission = await unitOfWork.Submissions.GetByIdAsync(submissionId)
            ?? throw new SubmissionNotFoundException(submissionId);

        if (!isTeacher && submission.StudentId != userId)
            throw new SubmissionNotFoundException(submissionId);

        var feedbacks = await unitOfWork.Feedbacks.GetBySubmissionIdAsync(submissionId);
        return feedbacks.Select(MapToDto).ToList();
    }

    private static FeedbackDto MapToDto(Feedback feedback) => new()
    {
        Id = feedback.Id,
        Text = feedback.Text,
        CreatedAt = feedback.CreatedAt,
        TeacherId = feedback.TeacherId,
        TeacherName = feedback.Teacher.Name
    };
}


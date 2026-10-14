using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.SubmissionDtos;
using Service.Contracts;

namespace LMS.Services;

public class SubmissionService : ISubmissionService
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmissionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StudentSubmissionDto> GetForStudentAsync(Guid activityId, string studentId)
    {
        var activity = await GetAssignmentForStudentAsync(activityId, studentId);
        var submission = await _unitOfWork.Submissions.GetByActivityAndStudentAsync(activityId, studentId);

        return MapToDto(activity, submission, DateTime.UtcNow);
    }

    private async Task<Activity> GetAssignmentForStudentAsync(Guid activityId, string studentId)
    {
        var activity = await _unitOfWork.Activities.GetByIdAsync(activityId)
            ?? throw new ActivityNotFoundException(activityId);

        var module = await _unitOfWork.Modules.GetByIdAsync(activity.ModuleId)
            ?? throw new ActivityNotFoundException(activityId);

        var memberships = await _unitOfWork.CourseUsers.GetByUserIdAsync(studentId);

        if (!memberships.Any(cu => cu.CourseId == module.CourseId))
            throw new ActivityNotFoundException(activityId);

        if (activity.ActivityType.Name != ActivityTypeNames.Assignment)
            throw new BadRequestException("Only an assignment can be handed in to.");

        return activity;
    }

    private static StudentSubmissionDto MapToDto(Activity activity, Submission? submission, DateTime now)
    {
        var files = submission?.Documents
            .Select(d => new SubmissionFileDto(d.Id, d.Name))
            .ToList() ?? [];

        // The "late" rule lives in the calculator, so the flag and the status word can never disagree
        var isLate = submission is not null && SubmissionStatusCalculator.IsLate(submission, activity.EndTime);

        return new StudentSubmissionDto(
            submission?.Id,
            activity.Id,
            activity.Name,
            activity.EndTime,
            submission?.SubmittedAt,
            isLate,
            SubmissionStatusCalculator.Calculate(submission, activity.EndTime, now),
            files);
    }
}
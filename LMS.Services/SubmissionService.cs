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
    private readonly IDocumentService _documentService;

    // Both are scoped, so they share the same unit of work (and DbContext) within one request
    public SubmissionService(IUnitOfWork unitOfWork, IDocumentService documentService)
    {
        _unitOfWork = unitOfWork;
        _documentService = documentService;
    }

    public async Task<StudentSubmissionDto> GetForStudentAsync(Guid activityId, string studentId)
    {
        var activity = await GetAssignmentForStudentAsync(activityId, studentId);
        var submission = await _unitOfWork.Submissions.GetByActivityAndStudentAsync(activityId, studentId);

        return MapToDto(activity, submission, DateTime.UtcNow);
    }

    public async Task<StudentSubmissionDto> SubmitAsync(
        Guid activityId,
        string studentId,
        Stream content,
        string fileName,
        string contentType,
        long length,
        CancellationToken cancellationToken)
    {
        var activity = await GetAssignmentForStudentAsync(activityId, studentId);
        var submission = await _unitOfWork.Submissions.GetByActivityAndStudentAsync(activityId, studentId);

        // Checked before the file is stored, so a locked submission never gets a stray file on disk (#60)
        if (submission is not null && SubmissionStatusCalculator.IsLocked(submission))
            throw new ConflictException(
                "The teacher has already given feedback on this assignment, so no more files can be handed in.",
                "Submission locked");

        // First hand-in: create the row. It is only added here, not saved yet.
        // The id is set now because the document below needs it as its foreign key.
        if (submission is null)
        {
            submission = new Submission
            {
                Id = Guid.NewGuid(),
                ActivityId = activityId,
                StudentId = studentId
            };
            _unitOfWork.Submissions.Create(submission);
        }

        // The server sets the time, never the client. Every new file counts as handing in again.
        submission.SubmittedAt = DateTime.UtcNow;

        // Validates and stores the file, then saves the document AND the submission in one SaveChangesAsync.
        // If the file is rejected nothing is saved, so no empty submission is ever left behind.
        await _documentService.UploadAsync(
            content,
            fileName,
            contentType,
            length,
            courseId: null,
            moduleId: null,
            activityId: null,
            submissionId: submission.Id,
            studentId,
            cancellationToken);

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
        // Newest first. Sorted here too, because a file added in this request is appended last.
        var files = submission?.Documents
            .OrderByDescending(d => d.UploadedAt)
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
            submission is not null && SubmissionStatusCalculator.IsLocked(submission),
            files);
    }
}
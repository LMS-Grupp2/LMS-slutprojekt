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

    /// <summary>
    /// Teacher overview of one assignment (US10): one row per student in the assignment's course,
    /// including students who have not handed in. Each row gets its status from SubmissionStatusCalculator.
    /// 404 if the activity does not exist, 400 if it is not an assignment.
    /// </summary>
    public async Task<SubmissionOverviewDto> GetOverviewAsync(Guid activityId)
    {
        // 1. The assignment: must exist and be of type Assignment
        var activity = await _unitOfWork.Activities.GetByIdAsync(activityId)
            ?? throw new ActivityNotFoundException(activityId);

        if (activity.ActivityType.Name != ActivityTypeNames.Assignment)
            throw new BadRequestException("Only an assignment has submissions");

        // The module tells us which course the assignment belongs to
        var module = await _unitOfWork.Modules.GetByIdAsync(activity.ModuleId)
            ?? throw new ActivityNotFoundException(activityId);


        // 2. The students of that course. Teachers can also be course members, so filter by role.
        //    Two calls in total (members + all roles), then the filter runs in memory.
        var members = await _unitOfWork.CourseUsers.GetByCourseIdAsync(module.CourseId);
        var roleNames = await _unitOfWork.Users.GetRoleNamesByUserIdAsync();

        var students = members
            .Where(m => roleNames.TryGetValue(m.UserId, out var role) && role == UserRoles.Student)
            .ToList();

        // 3. All hand-ins for this assignment in one call, keyed by student for fast lookup.
        //    Safe as a dictionary: the unique index allows one submission per student per activity.
        var submissions = await _unitOfWork.Submissions.GetByActivityIdAsync(activityId);
        var submissionsByStudent = submissions.ToDictionary(s => s.StudentId);

        // 4. One row per student (a left join in memory): no submission found means null,
        // which turns into not submitted or missing.
        var deadlineUtc = CourseTime.ToUtc(activity.EndTime);
        var now = DateTime.UtcNow;

        var rows = students
            .Select(s => MapToRow(s, submissionsByStudent.GetValueOrDefault(s.UserId), deadlineUtc,
            now))
            .ToList();

        // Deadline is sent as Swedish time, so the page can show it as it is
        return new SubmissionOverviewDto(activity.Id, activity.Name, activity.EndTime, deadlineUtc < now,  rows);
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

        // The deadline is stored as Swedish time, SubmittedAt and now are UTC: compare everything in UTC
        var deadlineUtc = CourseTime.ToUtc(activity.EndTime);

        // The "late" rule lives in the calculator, so the flag and the status word can never disagree
        var isLate = submission is not null && SubmissionStatusCalculator.IsLate(submission, deadlineUtc);

        return new StudentSubmissionDto(
            submission?.Id,
            activity.Id,
            activity.Name,
            activity.EndTime, // as entered (Swedish time), so the page shows it unconverted
            submission?.SubmittedAt,
            isLate,
            SubmissionStatusCalculator.Calculate(submission, deadlineUtc, now),
            submission is not null && SubmissionStatusCalculator.IsLocked(submission),
            files);
    }

    /// <summary>
    /// One student + their submission (null if none) → one row for the teacher's table.
    /// </summary>
    private static SubmissionOverviewRowDto MapToRow(CourseUser student, Submission? submission,
        DateTime deadlineUtc, DateTime now)
    {
        var files = submission?.Documents
            .Select(d => new SubmissionFileDto(d.Id, d.Name))
            .ToList() ?? [];

        return new SubmissionOverviewRowDto(
            student.UserId,
            student.User.Name,
            submission?.SubmittedAt,
            submission?.Approved ?? false,
            submission?.NeedChanges ?? false,
            SubmissionStatusCalculator.Calculate(submission, deadlineUtc, now),
            files
        );
    }
}
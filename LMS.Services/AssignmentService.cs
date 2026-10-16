using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.DTOs.AssignmentDtos;
using Service.Contracts;

namespace LMS.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IUnitOfWork _unitOfWork;
    public AssignmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Gets the assignments for the specified student as a collection of StudentAssignmentDto values.
    /// </summary>
    /// <remarks>Students assignments converted to StudentAssignmentDto, including module and course information
    /// and mapping the latest submission (Submitted, SubmissionId, SubmittedAt, Status, Grade, HasFeedback). Returns an
    /// empty collection if no assignments are found.</remarks>
    /// <param name="studentId">The student identifier.</param>
    /// <returns>A task result contains a collection of StudentAssignmentDto for the student.</returns>
    public async Task<IEnumerable<StudentAssignmentDto>> GetAssignmentsForStudentAsync(string studentId)
    {
        IEnumerable<Activity> activities = await _unitOfWork.Assignment.GetAssignmentsForStudentAsync(studentId);

        //The same cloc for every assignment in the list.
        var now = DateTime.UtcNow;

        var activityList = activities.Select(a =>
        {
            var submission = a.Submissions.FirstOrDefault();

            //The deadline is stored as Swedish time we convert it before we compare
            var deadlineUtc = CourseTime.ToUtc(a.EndTime);

            return new StudentAssignmentDto
            {
                ActivityId = a.Id,
                ActivityName = a.Name,
                ActivityDescription = a.Description,
                EndTime = a.EndTime,
                ModuleId = a.Module.Id,
                ModuleName = a.Module.Name,
                CourseId = a.Module.Course.Id,
                CourseName = a.Module.Course.Name,
                Submitted = submission is not null,
                SubmissionId = submission?.Id,
                SubmittedAt = submission?.SubmittedAt,
                Status = SubmissionStatusCalculator.Calculate(submission, deadlineUtc, now),
                Grade = submission?.Grade?.ToString(),
                HasFeedback = submission?.Feedbacks.Any() ?? false

            };
        })
        .ToList();

        return activityList;
    }
}
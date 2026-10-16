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

        var activityList = activities.Select(a =>
        {
            var submission = a.Submissions.FirstOrDefault();
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
                Status = submission is null
                    ? "Not submitted"
                    : submission.Approved ? "Approved" : submission.NeedChanges ? "Needs changes" : "Submitted",
                Grade = submission?.Grade?.ToString(),
                HasFeedback = (submission?.Feedbacks.Any() ?? false) ? true : false
            };
        })
        .ToList();

        return activityList;
    }
}
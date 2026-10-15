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
                Status = submission is null ? "Not submitted" : "Submitted",
                Grade = submission?.Grade?.ToString()
            };
        })
        .ToList();

        return activityList;
    }
}
using LMS.Shared.DTOs;

namespace Service.Contracts;

/// <summary> Get studens assignments with status</summary>
public interface IAssignmentService
{
    Task<IEnumerable<StudentAssignmentDto>> GetAssignmentsForStudentAsync(string studentId);
}
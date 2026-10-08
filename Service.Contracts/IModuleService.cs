using LMS.Shared.DTOs.ModuleDtos;

namespace Service.Contracts;

/// <summary>Business operations for modules, used by the modules controller.</summary>
public interface IModuleService
{
    /// <summary>
    /// Lists the modules of a course, ordered by start date.
    /// Teachers can read any course. A student can only read the course they belong to,
    /// for any other course a 404 is returned so the course is not revealed.
    /// </summary>
    Task<IEnumerable<ModuleDto>> GetByCourseIdAsync(Guid courseId, string userId, bool isTeacher);

    /// <summary>Gets one module. Throws a 404 if it does not exist. Used by teachers.</summary>
    Task<ModuleDto> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a module in a course. Throws a 400 for invalid dates or dates outside the course,
    /// and a 409 if the module overlaps another module in the same course.
    /// </summary>
    Task<ModuleDto> CreateAsync(Guid courseId, CreateModuleDto dto);

    /// <summary>Updates a module. Same rules as CreateAsync, and a 404 if the module does not exist.</summary>
    Task UpdateAsync(Guid id, UpdateModuleDto dto);
}
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.ModuleDtos;
using Service.Contracts;

namespace LMS.Services;

/// <summary>
/// Business rules for modules. Controllers call this service,
/// and it talks to the database only through the unit of work.
/// </summary>
public class ModuleService(IUnitOfWork unitOfWork) : IModuleService
{
    /// <summary>
    /// Lists the modules of a course. A student can only read the course they belong to.
    /// </summary>
    public async Task<IEnumerable<ModuleDto>> GetByCourseIdAsync(Guid courseId, string userId, bool isTeacher)
    {
        // A student asking for another course gets the same 404 as for a course that does not exist,
        // so we do not reveal that the course exists.
        if (!await CanAccessCourseAsync(courseId, userId, isTeacher))
            throw new CourseNotFoundException(courseId);

        _ = await unitOfWork.Courses.GetCourseByIdAsync(courseId)
            ?? throw new CourseNotFoundException(courseId);

        var modules = await unitOfWork.Modules.GetByCourseIdAsync(courseId);
        return modules.Select(MapToDto);
    }

    /// <summary>Returns one module, or throws a 404 if it does not exist.</summary>
    public async Task<ModuleDto> GetByIdAsync(Guid id)
    {
        var module = await unitOfWork.Modules.GetByIdAsync(id)
            ?? throw new ModuleNotFoundException(id);

        return MapToDto(module);
    }

    /// <summary>Creates a module after checking the dates against the course and the other modules.</summary>
    public async Task<ModuleDto> CreateAsync(Guid courseId, CreateModuleDto dto)
    {
        var course = await unitOfWork.Courses.GetCourseByIdAsync(courseId)
            ?? throw new CourseNotFoundException(courseId);

        var start = dto.StartDate!.Value;
        var end = dto.EndDate!.Value;
        await ValidateDatesAsync(start, end, course, excludeModuleId: null);

        var module = new Module
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            StartDate = start,
            EndDate = end,
            CourseId = courseId
        };

        unitOfWork.Modules.Create(module);
        await unitOfWork.SaveChangesAsync();

        return MapToDto(module);
    }

    /// <summary>Updates a module. The same date rules as for creating apply.</summary>
    public async Task UpdateAsync(Guid id, UpdateModuleDto dto)
    {
        // The module is tracked, so changing the properties and saving is enough.
        var module = await unitOfWork.Modules.GetByIdAsync(id, trackChanges: true)
            ?? throw new ModuleNotFoundException(id);

        var course = await unitOfWork.Courses.GetCourseByIdAsync(module.CourseId)
            ?? throw new CourseNotFoundException(module.CourseId);

        var start = dto.StartDate!.Value;
        var end = dto.EndDate!.Value;

        // The module itself is excluded from the overlap check,
        // so it can be saved without changing its dates.
        await ValidateDatesAsync(start, end, course, excludeModuleId: module.Id);

        module.Name = dto.Name.Trim();
        module.Description = dto.Description?.Trim();
        module.StartDate = start;
        module.EndDate = end;

        await unitOfWork.SaveChangesAsync();
    }

    /* Helper methods */

    /// <summary>Teachers can access every course. A student can only access a course they belong to.</summary>
    private async Task<bool> CanAccessCourseAsync(Guid courseId, string userId, bool isTeacher)
    {
        if (isTeacher) return true;

        var memberships = await unitOfWork.CourseUsers.GetByUserIdAsync(userId);
        return memberships.Any(cu => cu.CourseId == courseId);
    }

    /// <summary>
    /// 400 if the dates are in the wrong order or outside the course,
    /// 409 if the module overlaps another module in the same course.
    /// </summary>
    private async Task ValidateDatesAsync(DateOnly start, DateOnly end, Course course, Guid? excludeModuleId)
    {
        if (start > end)
            throw new BadRequestException(
                $"Start date {start:yyyy-MM-dd} cannot be after end date {end:yyyy-MM-dd}.",
                "Invalid dates");

        if (start < course.StartDate || end > course.EndDate)
            throw new BadRequestException(
                $"Module dates must lie within the course period {course.StartDate:yyyy-MM-dd} to {course.EndDate:yyyy-MM-dd}.",
                "Invalid dates");

        var overlapping = await unitOfWork.Modules.FindOverlappingAsync(
            course.Id, start, end, excludeModuleId);

        if (overlapping is not null)
            throw new ConflictException(
                $"The module overlaps with '{overlapping.Name}' ({overlapping.StartDate:yyyy-MM-dd} to {overlapping.EndDate:yyyy-MM-dd}).",
                "Module overlaps");
    }

    /// <summary>Single place that maps a Module entity to its DTO.</summary>
    private static ModuleDto MapToDto(Module m) =>
        new(m.Id, m.Name, m.Description, m.StartDate, m.EndDate, m.CourseId);
}
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.ModuleDtos;
using Service.Contracts;

namespace LMS.Services;

public class ModuleService(IUnitOfWork unitOfWork) : IModuleService
{
    public async Task<IEnumerable<ModuleDto>> GetByCourseIdAsync(Guid courseId)
    {
        _ = await unitOfWork.Courses.GetCourseByIdAsync(courseId)
            ?? throw new CourseNotFoundException(courseId);

        var modules = await unitOfWork.Modules.GetByCourseIdAsync(courseId);
        return modules.Select(MapToDto);
    }

    public async Task<ModuleDto> GetByIdAsync(Guid id)
    {
        var module = await unitOfWork.Modules.GetByIdAsync(id)
            ?? throw new ModuleNotFoundException(id);

        return MapToDto(module);
    }

    public async Task<ModuleDto> CreateAsync(Guid courseId, CreateModuleDto dto)
    {
        var course = await unitOfWork.Courses.GetCourseByIdAsync(courseId)
            ?? throw new CourseNotFoundException(courseId);

        var start = dto.StartDate!.Value;
        var end = dto.EndDate!.Value;
        ValidateDates(start, end, course);

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

    public async Task UpdateAsync(Guid id, UpdateModuleDto dto)
    {
        var module = await unitOfWork.Modules.GetByIdAsync(id, trackChanges: true)
            ?? throw new ModuleNotFoundException(id);

        var course = await unitOfWork.Courses.GetCourseByIdAsync(module.CourseId)
            ?? throw new CourseNotFoundException(module.CourseId);

        var start = dto.StartDate!.Value;
        var end = dto.EndDate!.Value;
        ValidateDates(start, end, course);

        module.Name = dto.Name.Trim();
        module.Description = dto.Description?.Trim();
        module.StartDate = start;
        module.EndDate = end;

        await unitOfWork.SaveChangesAsync();
    }

    /* Helper methods */

    private static void ValidateDates(DateOnly start, DateOnly end, Course course)
    {
        if (start > end)
            throw new BadRequestException(
                $"Start date {start:yyyy-MM-dd} cannot be after end date {end:yyyy-MM-dd}.",
                "Invalid dates");

        if (start < course.StartDate || end > course.EndDate)
            throw new BadRequestException(
                $"Module dates must lie within the course period {course.StartDate:yyyy-MM-dd} to {course.EndDate:yyyy-MM-dd}.",
                "Invalid dates");
    }

    private static ModuleDto MapToDto(Module m) =>
        new(m.Id, m.Name, m.Description, m.StartDate, m.EndDate, m.CourseId);
}

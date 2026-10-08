namespace LMS.Shared.DTOs.CourseDtos;

/// <summary>
/// Input for updating a course. It has the same fields and rules as CreateCourseDto.
/// The course id is not part of the body, it comes from the route (PUT api/courses/{id}),
/// so the two can never disagree.
/// </summary>
public record UpdateCourseDto : CreateCourseDto;
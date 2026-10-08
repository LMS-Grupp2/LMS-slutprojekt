namespace Domain.Models.Exceptions;

public class CourseNotFoundException : NotFoundException
{
    public CourseNotFoundException()
        : base("You are not enrolled in any course.", "Course not found")
    {
    }

    public CourseNotFoundException(Guid id)
        : base($"Course with id '{id}' was not found.", "Course not found")
    {
    }
}
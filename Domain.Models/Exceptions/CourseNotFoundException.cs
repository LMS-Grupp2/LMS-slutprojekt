namespace Domain.Models.Exceptions;

public class CourseNotFoundException : NotFoundException
{
    public CourseNotFoundException(string message = "Course not found", string title = "Course not found")
        : base(message, title)
    {
    }

    public CourseNotFoundException(Guid id)
        : base($"Course with id '{id}' was not found.", "Course not found")
    {
    }
}
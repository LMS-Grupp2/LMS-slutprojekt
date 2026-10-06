namespace Domain.Models.Exceptions;

public class CourseNotFoundException : NotFoundException
{
    public CourseNotFoundException()
        : base("You are not enrolled in any course.", "Course not found")
    {
    }
}

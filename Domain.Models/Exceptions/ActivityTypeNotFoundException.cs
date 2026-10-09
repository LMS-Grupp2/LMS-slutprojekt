
namespace Domain.Models.Exceptions;

public class ActivityTypeNotFoundException : NotFoundException
{
    public ActivityTypeNotFoundException(Guid id)
        : base($"Activity type with '{id}' was not found", "Activity type not found.")
    {

    }
}

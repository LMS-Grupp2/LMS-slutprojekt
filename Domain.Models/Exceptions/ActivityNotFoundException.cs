
namespace Domain.Models.Exceptions;

public class ActivityNotFoundException : NotFoundException
{
   public ActivityNotFoundException(Guid id) :
        base($"Activity with id '{id}' was not found.", "Activity not found.")
    {

    }
}

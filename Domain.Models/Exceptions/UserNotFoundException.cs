
namespace Domain.Models.Exceptions;

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(string id)
        : base($"User with id '{id}' was not found.", "No user found.")
    {

    }
}

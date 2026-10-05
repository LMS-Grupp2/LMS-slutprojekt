

namespace Domain.Models.Exceptions;

public class ConflictException : Exception
{
    public string Title { get; }

    public ConflictException(string message, string title = "Conflict") : base(message)
    {
        Title = title;
    }
}

namespace Domain.Models.Exceptions;

public class NotificationNotFoundException : NotFoundException
{
    public NotificationNotFoundException(string message = "Notification was not found.")
        : base(
            $"{message}",
            "Notification not found.")
    {
    }
}

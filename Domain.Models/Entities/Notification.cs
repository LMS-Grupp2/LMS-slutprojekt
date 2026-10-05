namespace Domain.Models.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public string RecipientId { get; set; } = null!;
    public ApplicationUser Recipient { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;


namespace LMS.Shared.DTOs.ActivityDtos;

/// <summary>
/// Input for PUT api/activities/{id}. No ModuleId on purpose: an activity can't move
/// to another module, so the service takes the module from the stored activity.
/// </summary>
public record UpdateActivityDto 
{
    [Required]
    public Guid? ActivityTypeId { get; init; } 

    [Required]
    [MaxLength(50)]
    public string Name { get; init; } = null!;

    [MaxLength(400)]
    public string? Description { get; init; }

    [Required]
    public DateTime? StartTime { get; init; }

    [Required]
    public DateTime? EndTime { get; init; }

}

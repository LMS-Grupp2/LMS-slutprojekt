using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.NotificationDtos;
using Service.Contracts;

namespace LMS.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Creates notifications for one or more students
    /// </summary>
    public async Task CreateNotificationAsync(CreateNotificationDto dto)
    {
        // Get the student Ids that should receive the notification.
        IEnumerable<string> studentIds = await GetUserIdnAsync(dto);

        // Create notification for each studentId
        foreach (string studentId in studentIds)
        {
            var notification = new Notification
            {
                EventType = dto.EventType,
                Message = dto.Message,
                CreatedAt = DateTime.UtcNow,
                RecipientId = studentId
            };
            _unitOfWork.Notification.Add(notification);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Get list of all notifications for student
    /// </summary>
    /// /// <returns>List of students notifications</returns>
    public async Task<IEnumerable<NotificationDto>> GetByStudentIdAsync(string studentId, int nbrItems)
    {
        var result = await _unitOfWork.Notification.GetByStudentIdAsync(studentId, nbrItems);

        var dtos = new List<NotificationDto>();

        foreach (var note in result)
        {
            dtos.Add(
                new NotificationDto
                {
                    Id = note.Id,
                    EventType = note.EventType,
                    Message = note.Message,
                    IsRead = note.IsRead,
                    CreatedAt = note.CreatedAt
                }
            );
        }

        return dtos;
    }

    /// <summary>
    /// Updates single notification to be IsRead
    /// Toggle single notification to be 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    /// <exception cref="NotificationNotFoundException"></exception>
    public async Task<bool> UpdateNotification(string userId, UpdateNotificationDto dto)
    {
        if (dto.Id is null)
            throw new BadRequestException("Notification is not set");

        var notification = await _unitOfWork.Notification.GetByIdAsync(userId, (Guid)dto.Id);

        if (notification is null)
            throw new NotificationNotFoundException();

        // Changes - can only change IsRead at the moment
        notification.IsRead = dto.IsRead ?? false;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAllRead(string userId)
    {
        var notification = await _unitOfWork.Notification.GetAllUnreadByStudentIdAsync(userId);
        if (notification is null)
            throw new NotificationNotFoundException();

        foreach (var item in notification)
        {
            item.IsRead = true;
        }
        await _unitOfWork.SaveChangesAsync();

        return true;
    }


    /* Helpers */

    /// <summary>
    /// Returns ids for all students that shall have a notification. Gets user from dtos RecipientId, CourseId, ModuleId or ActivityId
    /// The result can be one or may student ids
    /// </summary>
    /// <returns>List of ids for student in a course</returns>
    private async Task<IEnumerable<string>> GetUserIdnAsync(CreateNotificationDto dto)  //(string? CourseId, string? ModuleId, string ActivityId)
    {
        IEnumerable<string> idList = [];

        if (!string.IsNullOrWhiteSpace(dto.RecipientId))
        {
            // Feedback to RecipientId
            idList = [dto.RecipientId];
        }
        else if (dto.CourseId is not null)
        {
            idList = await _unitOfWork.Notification.GetStudentsIdnByCourseIdAsync((Guid)dto.CourseId);
        }
        else if (dto.ModuleId is not null)
        {
            idList = await _unitOfWork.Notification.GetStudentsIdnByModuleIdAsync((Guid)dto.ModuleId);
        }
        else if (dto.ActivityId is not null)
        {
            idList = await _unitOfWork.Notification.GetStudentsIdnByActivityIdAsync((Guid)dto.ActivityId);
        }

        return idList;
    }
}

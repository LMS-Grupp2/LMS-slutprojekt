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

    // Todo
    // Create notification when new feedback or new course document
    // Document - students by courseId/moduleId/...
    // Feedback - studentId
    public async Task CreateNotificationAsync(CreateNotificationDto dto)
    {
        // Get the student IDs that should receive the notification.
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

    public async Task<IEnumerable<NotificationDto>> GetByStudentIdAsync(string studentId)
    {
        var result = await _unitOfWork.Notification.GetByStudentId(studentId);

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
                    CreatedAd = note.CreatedAt
                }
            );
        }

        return dtos;
    }

    public async Task<bool> SetIsRead(Guid notificatonId)
    {
        var notification = await _unitOfWork.Notification.GetById(notificatonId);

        if (notification is null)
            throw new BadRequestException("Unknown notification");

        notification.IsRead = true;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }


    /* Helpers */

    // Get  To whitch users to add notification. Single or to whole class
    public async Task<IEnumerable<string>> GetUserIdnAsync(CreateNotificationDto dto)  //(string? CourseId, string? ModuleId, string ActivityId)
    {
        var idList = new List<string>();

        if (!string.IsNullOrWhiteSpace(dto.RecipientId))
        {
            // Feedback to RecipientId
            idList.Add(dto.RecipientId);
        }
        else if (!string.IsNullOrWhiteSpace(dto.CourseId))
        {
            //idList = await GetStudentsIdnByCourseIdAsync(dto.CourseId);
        }
        else if (!string.IsNullOrWhiteSpace(dto.ModuleId))
        {
            //idList = await GetStudentsIdnByModuleIdAsync(dto.ModuleId);
        }
        else if (!string.IsNullOrWhiteSpace(dto.ActivityId))
        {
            //idList = await GetStudentsIdnByActivityIdAsync(dto.ActivityId);
        }

        return idList;
    }

}

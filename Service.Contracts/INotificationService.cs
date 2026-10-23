using LMS.Shared.DTOs.NotificationDtos;

namespace Service.Contracts;

public interface INotificationService
{
    Task CreateNotificationAsync(CreateNotificationDto dto);
    Task<IEnumerable<NotificationDto>> GetByStudentIdAsync(string studentId, int nbrItems);
    Task<bool> UpdateAllRead(string userId);
    Task<bool> UpdateNotification(string userId, UpdateNotificationDto dto);
}
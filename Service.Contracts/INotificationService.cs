using LMS.Shared.DTOs.NotificationDtos;

namespace Service.Contracts;

public interface INotificationService
{
    Task CreateNotificationAsync(CreateNotificationDto dto);
    Task<IEnumerable<NotificationDto>> GetByStudentIdAsync(string studentId);
    Task<bool> UpdateNotification(UpdateNotificationDto dto);
}
using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface INotificationRepository
{
    void Add(Notification notification);
    Task<Notification?> GetByIdAsync(string userId, Guid id);
    Task<IEnumerable<Notification>> GetAllUnreadByStudentIdAsync(string userId);
    Task<IEnumerable<Notification>> GetByStudentIdAsync(string studentId, int nbrItems);
    Task<IEnumerable<string>> GetStudentsIdnByActivityIdAsync(Guid activityId);
    Task<IEnumerable<string>> GetStudentsIdnByCourseIdAsync(Guid courseId);
    Task<IEnumerable<string>> GetStudentsIdnByModuleIdAsync(Guid moduleId);
}
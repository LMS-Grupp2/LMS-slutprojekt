using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface INotificationRepository
{
    void Add(Notification notification);
    Task<Notification?> GetById(string userId, Guid id);
    Task<IEnumerable<Notification>> GetByStudentId(string studentId, int nbrItems);
    Task<IEnumerable<string>> GetStudentsIdnByActivityIdAsync(Guid activityId);
    Task<IEnumerable<string>> GetStudentsIdnByCourseIdAsync(Guid courseId);
    Task<IEnumerable<string>> GetStudentsIdnByModuleIdAsync(Guid moduleId);
}
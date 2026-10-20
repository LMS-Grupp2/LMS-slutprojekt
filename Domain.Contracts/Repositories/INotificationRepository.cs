using Domain.Models.Entities;

namespace LMS.Infrastructure.Repositories
{
    public interface INotificationRepository
    {
        void Add(Notification notification);
        Task<Notification?> GetById(Guid id);
        Task<IEnumerable<Notification>> GetByStudentId(string studentId);
        Task<IEnumerable<string>> GetStudentsIdnByActivityIdAsync(Guid activityId);
        Task<IEnumerable<string>> GetStudentsIdnByCourseIdAsync(Guid courseId);
        Task<IEnumerable<string>> GetStudentsIdnByModuleIdAsync(Guid moduleId);
    }
}
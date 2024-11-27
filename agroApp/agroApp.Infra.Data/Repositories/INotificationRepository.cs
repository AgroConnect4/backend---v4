using agroApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetNotificationsByUserIdAsync(Guid userId);
        Task AddNotificationAsync(Notification notification);
        Task<Notification> GetLatestConnectionNotificationByUserIdAsync(Guid userId);
    }
}
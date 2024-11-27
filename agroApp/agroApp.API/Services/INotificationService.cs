using agroApp.Domain.Entities;
using System.Threading.Tasks;

namespace agroApp.API.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(Notification notification);
        Task<List<Notification>> GetNotificationsByUserIdAsync(Guid userId);
        Task SendPostCreatedNotificationAsync(Guid connectedUserId, Post post);
        //Task SendConnectionRequestNotificationAsync(Guid userId, User user);
        Task SendConnectionRequestNotificationAsync(Guid connectedUserId, User user);
        Task SendConnectionAcceptedNotificationAsync(Guid userId, User connectedUser, bool isSender);
        Task SendConnectionRejectedNotificationAsync(Guid userId, User connectedUser);
        Task<Notification> GetLatestConnectionNotificationAsync(Guid userId);
        Task SendEventCreatedNotificationAsync(Guid userId, Event @event);
        Task SendEventJoinedNotificationAsync(Guid userId, Event @event, Guid participantId);
        Task SendEventCommentedNotificationAsync(Guid userId, Event @event, Guid commenterId, Guid commentId);
        Task SendPostCommentedNotificationAsync(Guid userId, Post post, Guid commenterId, Guid commentId);
    }
}
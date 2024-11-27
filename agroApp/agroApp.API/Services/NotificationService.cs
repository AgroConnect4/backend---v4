using agroApp.Domain.Entities;
using agroApp.Infra.Data.Repositories;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace agroApp.API.Services
{
    public class NotificationService : INotificationService
    {
       private readonly INotificationRepository _notificationRepository;
       private readonly IUserRepository _userRepository;
       private readonly IConnectionRepository _connectionRepository;
       private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger,
            IUserRepository userRepository,
            IConnectionRepository connectionRepository,
            INotificationRepository notificationRepository
            )
        {
            _logger = logger;
            _connectionRepository = connectionRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        public async Task SendPostCreatedNotificationAsync(Guid connectedUserId, Post post)
        {
            var user = await _userRepository.GetByIdAsync(post.UserId);
            var notification = new Notification
            {
                UserId = connectedUserId,
                Message = $"O usuário {user?.UserName ?? "Usuário desconhecido"} criou um novo post: {post.Title}", // Adicione o título do post
                CreatedAt = DateTime.UtcNow,
                Type = NotificationType.PostCreated // adicione o tipo de notificação
            };
            try
            {
                await CreateNotificationAsync(notification);
                _logger.LogInformation($"SendPostCreatedNotificationAsync({connectedUserId}, {post.Id}): Notification created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SendPostCreatedNotificationAsync({connectedUserId}, {post.Id}): Error creating notification.");
            }
        }

        public async Task SendConnectionRequestNotificationAsync(Guid connectedUserId, User user)
        {
            var notification = new Notification
            {
                UserId = connectedUserId,
                Message = $"{user?.UserName ?? "An unknown user"} wants to connect with you.",
                CreatedAt = DateTime.UtcNow,
                Type = NotificationType.ConnectionRequest
            };
            await CreateNotificationAsync(notification);
        }

        public async Task SendConnectionAcceptedNotificationAsync(Guid userId, User connectedUser, bool isSender)
        {
            string message = isSender 
                ? $"{connectedUser.UserName} accepted your connection request." 
                : $"{connectedUser.UserName}'s connection request was accepted.";

            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                Type = NotificationType.ConnectionAccepted
            };
            await CreateNotificationAsync(notification);
        }

        public async Task SendConnectionRejectedNotificationAsync(Guid userId, User connectedUser)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = $"{connectedUser?.UserName ?? "Um usuário desconhecido"} rejeitou sua solicitação de conexão.",
                CreatedAt = DateTime.UtcNow,
                Type = NotificationType.ConnectionRejected
            };
            await CreateNotificationAsync(notification);
        }

        public async Task SendEventCreatedNotificationAsync(Guid userId, Event @event)
        {
            var creator = await _userRepository.GetByIdAsync(@event.UserId);
            var notification = new Notification
            {
                UserId = userId,
                Message = $"O usuário {creator?.UserName ?? "Um usuário desconhecido"} criou um novo evento: {@event.Name}",
                CreatedAt = DateTime.UtcNow,
                Type = NotificationType.EventCreated
            };
            await CreateNotificationAsync(notification);
        }

        public async Task SendEventJoinedNotificationAsync(Guid userId, Event @event, Guid participantId)
        {
            var participant = await _userRepository.GetByIdAsync(participantId);
            var notification = new Notification
            {
                UserId = userId, // Event creator's ID
                Message = $"O usuário {participant?.UserName ?? "Um usuário desconhecido"} confirmou que vai participar do seu evento: {@event.Name}",
                CreatedAt = DateTime.UtcNow,
                Type = NotificationType.EventJoined
            };
            await CreateNotificationAsync(notification);
        }

        public async Task SendEventCommentedNotificationAsync(Guid userId, Event @event, Guid commenterId, Guid commentId)
        {
            var commenter = await _userRepository.GetByIdAsync(commenterId);
            var notification = new Notification
            {
                UserId = userId, // Event creator's ID
                Message = $"O usuário {commenter?.UserName ?? "Um usuário desconhecido"} comentou no seu evento: {@event.Name} (Comentário ID: {commentId})",
                CreatedAt = DateTime.UtcNow,
                Type = NotificationType.PostCommented // You might want a more specific type
            };
            await CreateNotificationAsync(notification);
        }

        public async Task SendPostCommentedNotificationAsync(Guid userId, Post post, Guid commenterId, Guid commentId)
        {
            var commenter = await _userRepository.GetByIdAsync(commenterId);
            var notification = new Notification
            {
                UserId = userId, // Post creator's ID
                Message = $"{commenter?.UserName ?? "An unknown user"} commented on your post: \"{post.Title}\" (Comment ID: {commentId})",
                CreatedAt = DateTime.UtcNow,
                Type = NotificationType.PostCommented
            };
            await CreateNotificationAsync(notification);
        }

        public async Task<Notification> GetLatestConnectionNotificationAsync(Guid userId)
        {
            return await _notificationRepository.GetLatestConnectionNotificationByUserIdAsync(userId);
        }
        

        public async Task CreateNotificationAsync(Notification notification)
        {
            await _notificationRepository.AddNotificationAsync(notification);
        }

        
        public async Task<List<Notification>> GetNotificationsByUserIdAsync(Guid userId) // Added method
        {
            return await _notificationRepository.GetNotificationsByUserIdAsync(userId);
        }
    }
}
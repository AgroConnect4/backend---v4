using agroApp.API.DTOs;
using agroApp.Domain.Entities;
using agroApp.Infra.Data.Repositories;
using Microsoft.AspNetCore.Http; 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // Importar o namespace do UserManager

namespace agroApp.API.Services
{
    public class EventCommentService : IEventCommentService
    {
        private readonly IEventCommentRepository _commentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly IEventRepository _eventRepository; // Add this
        private readonly IUserRepository _userRepository; // Add this
        private readonly INotificationService _notificationService; // Add this
        private readonly ILogger<EventCommentService> _logger;

        public EventCommentService(
            IEventCommentRepository commentRepository, 
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager,
            IEventRepository eventRepository, // Inject IEventRepository
            IUserRepository userRepository, // Inject IUserRepository
            INotificationService notificationService, // Inject INotificationService
            ILogger<EventCommentService> logger 
        ) // Injete o UserManager
        {
            _commentRepository = commentRepository;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Guid> CreateCommentAsync(Guid eventId, CreateEventCommentDto request, Guid userId)
        {
            // No need for user existence check here – the controller already did it!
            var comment = new EventComment
            {
                EventId = eventId,
                UserId = userId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddAsync(comment);
            
            var @event = await _eventRepository.GetEventByIdAsync(eventId); // Get event using eventId
            await SendCommentNotificationAsync(@event, comment); // Pass the event to the notification method

            return comment.Id;
        }

        // Separate notification method
        private async Task SendCommentNotificationAsync(Event @event, EventComment comment)
        {
            if (@event != null)
            {
                var creatorId = @event.UserId;
                if (creatorId != comment.UserId) //Avoid notifying the commenter themselves.
                {
                    await _notificationService.SendEventCommentedNotificationAsync(creatorId, @event, comment.UserId, comment.Id);
                }
            }
            else
            {
                _logger.LogWarning($"Event with ID {comment.EventId} not found for notification.");
            }
        }

        public async Task<EventComment> UpdateCommentAsync(Guid commentId, UpdateEventCommentDto request, Guid userId)
        {
            // Validação do request
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Os dados da atualização do comentário não podem ser nulos.");
            }

            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.UserId != userId)
            {
                return null; // Or throw an exception
            }

            // Validação do ID do usuário (se necessário)
            // ...

            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.UtcNow; // Atualiza a data de atualização
            return await _commentRepository.UpdateAsync(comment);
        }

        public async Task DeleteCommentAsync(Guid commentId , Guid userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment != null && comment.UserId == userId)
            {
                await _commentRepository.DeleteAsync(commentId);
            }
        }

        public async Task<List<EventComment>> GetCommentsByEventIdAsync(Guid eventId)
        {
            return await _commentRepository.GetCommentsByEventIdAsync(eventId);
        }

        public async Task<EventComment> GetCommentByIdAsync(Guid commentId)
        {
            return await _commentRepository.GetByIdAsync(commentId);
        }

        public async Task<List<EventComment>> GetAllCommentsByEventIdAsync(Guid eventId)
        {
            return await _commentRepository.GetCommentsByEventIdAsync(eventId);
        }
    }
}
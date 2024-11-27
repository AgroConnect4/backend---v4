using agroApp.Domain.Entities;
using agroApp.Infra.Data.Context; // Assuma que você tem um contexto de banco de dados
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;
        
       private readonly ILogger<NotificationRepository> _logger;

        public NotificationRepository(AppDbContext context, 
            ILogger<NotificationRepository> logger)
        {
            _logger = logger;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification> GetLatestConnectionNotificationByUserIdAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && (n.Type == NotificationType.ConnectionRequest || 
                                                    n.Type == NotificationType.ConnectionAccepted ||
                                                    n.Type == NotificationType.ConnectionRejected))
                .OrderByDescending(n => n.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"AddNotificationAsync: Notification added successfully.  Notification ID: {notification.Id}");
        }
    }
}
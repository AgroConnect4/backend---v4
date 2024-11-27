using agroApp.Domain.Entities;

namespace agroApp.API.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } // Add Type
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public Guid? SenderId { get; set; } // Add SenderId
        public int RelatedItemId { get; set; } // Add RelatedItemId
        public DateTime CreatedAt { get; set; } 
    }
}
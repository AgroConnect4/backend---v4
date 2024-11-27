using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace agroApp.Domain.Entities
{
    public class EventParticipant
    {
        [Key]
        public Guid Id { get; set; }

        public Guid EventId { get; set; }
        public virtual Event Event { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
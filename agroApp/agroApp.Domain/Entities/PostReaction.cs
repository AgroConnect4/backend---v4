using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using agroApp.Domain.Entities;

namespace agroApp.Domain.Entities
{
    public class PostReaction
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public ReactionType ReactionType { get; set; }
        public DateTime ReactedAt { get; set; }
        public virtual Post Post { get; set; }
        public virtual User User { get; set; }
    }
}
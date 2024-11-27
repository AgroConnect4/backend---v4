using System;
using System.ComponentModel.DataAnnotations;
using agroApp.Domain.Entities;

namespace agroApp.API.DTOs
{
    public class CreatePostReactionDto
    {
        [Required]
        public Guid PostId { get; set; }
        [Required]
        public virtual ReactionType ReactionType { get; set; }
    }
}
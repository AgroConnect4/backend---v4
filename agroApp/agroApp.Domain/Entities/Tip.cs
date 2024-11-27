using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization; 

namespace agroApp.Domain.Entities
{
    public class Tip
    {
        public Guid Id { get; set; } 

        public string Title { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; } // URL or path to the icon
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        public Guid AuthorId { get; set; } // Foreign key to the User table
        [JsonIgnore]
        public virtual User Author { get; set; }
        public string Category { get; set; } //e.g., "Fertilization", "Pest Control", etc.
        public bool IsPublished { get; set; } = true;
    }
}
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization; 

namespace agroApp.Domain.Entities
{
public class News
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string ImageUrl { get; set; } // URL or path to the image
        public string IconUrl { get; set; } // URL or path to the icon
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        public Guid AuthorId { get; set; } // Foreign key to the User table
        [JsonIgnore]
        public virtual User Author { get; set; }
        public string Summary { get; set; } // A short summary for display
        public string SourceUrl {get; set;} // URL to the original source (if applicable)
        public bool IsPublished { get; set; } = true; // Controls if the news is visible
    }
}
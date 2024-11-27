using agroApp.Domain.Entities;

namespace agroApp.API.DTOs
{
    public class TipDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public Guid AuthorId { get; set; }
        public string Category { get; set; }
        public bool IsPublished { get; set; }
    }
}
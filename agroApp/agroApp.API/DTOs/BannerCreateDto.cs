using agroApp.Domain.Entities;

namespace agroApp.API.DTOs
{
     public class BannerCreateDto
    {
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string LinkUrl { get; set; }
        public bool IsActive { get; set; } = true; // Default to active
        public int DisplayOrder { get; set; } = 0; // Default display order
    }
}
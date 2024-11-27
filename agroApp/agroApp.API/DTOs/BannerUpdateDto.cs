using agroApp.Domain.Entities;

namespace agroApp.API.DTOs
{
     public class BannerUpdateDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string LinkUrl { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
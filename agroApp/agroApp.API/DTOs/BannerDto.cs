using agroApp.Domain.Entities;

namespace agroApp.API.DTOs
{
public class BannerDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string LinkUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public Guid UserId { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}

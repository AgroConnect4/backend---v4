using agroApp.Domain.Entities;

namespace agroApp.API.DTOs
{
    public class PortfolioItemDto { 
        public string Title { get; set; } 
        public string Description { get; set; }
        public string ImageUrl { get; set; } 
        public string VideoUrl { get; set; } 
}
}
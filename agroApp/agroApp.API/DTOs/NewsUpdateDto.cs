using agroApp.Domain.Entities;

namespace agroApp.API.DTOs
{
    public class NewsUpdateDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string ImageUrl { get; set; }
        public string IconUrl { get; set; }
        public string Summary { get; set; }
        public string SourceUrl { get; set; }
        public bool IsPublished { get; set; }
    }
}
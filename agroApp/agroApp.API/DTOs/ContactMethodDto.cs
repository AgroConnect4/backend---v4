using agroApp.Domain.Entities;

namespace agroApp.API.DTOs
{
    public class ContactMethodDto {
        public string Type { get; set; }
        public string UrlOrNumber { get; set; } 
    }
}
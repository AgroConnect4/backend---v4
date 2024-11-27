using agroApp.Domain.Entities;

namespace agroApp.API.DTOs
{
    public class UpdateProfileDto
    {
        public string Name { get; set; }
        public string Bio { get; set; }
        public string ProfilePicture { get; set; }
        public string CoverPicture { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }   
        public List<string> Certifications { get; set; }
        public List<string> ProductsOffered { get; set; }
    }
}
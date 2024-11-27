using agroApp.Domain.Entities;

namespace agroApp.API.DTOs
{
    public class UserProfileDto
    {
       public Guid Id { get; set; }
       public string Name { get; set; } // Added - Profile's name
        public string Bio { get; set; }  // Added - Profile's bio
        public string ProfilePicture { get; set; } //Added
        public string CoverPicture { get; set; } //Added
        public string Description { get; set; } //Added
        public string PhoneNumber { get; set; } //Added
        public string Website { get; set; } //Added
        public double AverageRating { get; set; }
        public List<string> Certifications { get; set; }
        public List<string> ProductsOffered { get; set; }
        public List<ContactMethodDto> ContactMethods { get; set; }
        public List<FarmDto> Farms { get; set; }
        public List<SpecializationDto> Specializations { get; set; }
        public List<PortfolioItemDto> Portfolio { get; set; }
    }
}
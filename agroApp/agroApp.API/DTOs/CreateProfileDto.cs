using agroApp.Domain.Entities;

namespace agroApp.API.DTOs
{
    public class CreateProfileDto
    {
        public string Name { get; set; }
        public string Bio { get; set; }
        public string ProfilePicture { get; set; }
        public string CoverPicture { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
        public List<string> Certifications { get; set; } = new List<string>();
        public List<string> ProductsOffered { get; set; } = new List<string>();
        public List<FarmDto> Farms { get; set; } = new List<FarmDto>();
        public List<SpecializationDto> Specializations { get; set; } = new List<SpecializationDto>();
        public List<PortfolioItemDto> Portfolio { get; set; } = new List<PortfolioItemDto>();
        public List<ContactMethodDto> ContactMethods { get; set; } = new List<ContactMethodDto>();
    }
}
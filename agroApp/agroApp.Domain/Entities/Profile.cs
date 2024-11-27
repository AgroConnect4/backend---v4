using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace agroApp.Domain.Entities
{
    public class Profile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }

        public string Name { get; set; }  = "";
        public string Bio {get; set;}  = "";
        public string ProfilePicture {get; set;}  = "";
        public string CoverPicture {get; set;}  = "";
        public string Description {get; set;}  = "";

        [RegularExpression(@"^\(?([0-9]{2})\)?[-. ]?([0-9]{5})[-. ]?([0-9]{4})$", ErrorMessage = "Número de telefone inválido.")]
        public string PhoneNumber { get; set; }  = "";
        public string Website { get; set; }  = "";
        public double AverageRating { get; set; } = 0;

        public List<ContactMethod> ContactMethods { get; set; } = new List<ContactMethod>();
        public List<Farm> Farms { get; set; } = new List<Farm>();
        public List<Specialization> Specializations { get; set; } = new List<Specialization>();
        public List<PortfolioItem> Portfolio { get; set; } = new List<PortfolioItem>();

        public List<string> Certifications { get; set; } = new List<string>();
        public List<string> ProductsOffered { get; set; } = new List<string>();
        
        public Profile()
        {
            ContactMethods = new List<ContactMethod>();
            Farms = new List<Farm>();
            Specializations = new List<Specialization>();
            Portfolio = new List<PortfolioItem>();
            Certifications = new List<string>();
            ProductsOffered = new List<string>();
        }

    }

}

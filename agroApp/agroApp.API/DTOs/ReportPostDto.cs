using System.ComponentModel.DataAnnotations;

namespace agroApp.API.DTOs
{
    public class ReportPostDto
    {
        [Required]
        public string Reason { get; set; }
    }
}
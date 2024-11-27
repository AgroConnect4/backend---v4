using System;
using System.ComponentModel.DataAnnotations;

namespace agroApp.API.DTOs
{
    public class CreatePostReportDto
    {
        [Required]
        public Guid PostId { get; set; }
        [Required]
        public string Reason { get; set; }
    }
}
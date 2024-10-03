using System;
using System.ComponentModel.DataAnnotations;

namespace agroApp.API.DTOs
{
    public class UpdatePostDto
    {
        public string Content { get; set; }
        public string PostType { get; set; }
    }
}
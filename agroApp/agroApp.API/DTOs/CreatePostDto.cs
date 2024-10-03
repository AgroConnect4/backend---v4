using System;
using System.ComponentModel.DataAnnotations;

namespace agroApp.API.DTOs
{
    public class CreatePostDto
    {
        public string Content { get; set; }
        public string PostType { get; set; }
    }
}
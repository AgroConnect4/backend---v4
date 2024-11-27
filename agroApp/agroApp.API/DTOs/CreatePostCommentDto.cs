using System.ComponentModel.DataAnnotations;

namespace agroApp.API.DTOs
{
    public class CreatePostCommentDto
    {
        //[Required]
        //public int PostId { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
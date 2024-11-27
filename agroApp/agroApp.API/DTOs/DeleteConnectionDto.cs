using System.ComponentModel.DataAnnotations;

namespace agroApp.API.DTOs
{
    public class DeleteConnectionDto
    {
        public Guid ConnectedUserId { get; set; }
    }
}
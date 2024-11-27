using System.ComponentModel.DataAnnotations;

namespace agroApp.API.DTOs
{
    public class CreateConnectionDto
    {
        public Guid ConnectedUserId { get; set; }
    }
}
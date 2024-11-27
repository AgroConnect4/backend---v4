using agroApp.Domain.Entities;

namespace agroApp.API.DTOs
{
    public class AcceptRejectConnectionDto
    {
        public Guid ConnectedUserId { get; set; }
    }
}
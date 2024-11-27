using agroApp.API.DTOs; // Or whatever the correct namespace is

namespace agroApp.API.DTOs
{
    public class CreateAdminDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
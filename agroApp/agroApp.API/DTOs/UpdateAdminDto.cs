using System.ComponentModel.DataAnnotations;

namespace agroApp.API.DTOs
{
    public class UpdateAdminDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public bool AddAdminRole { get; set; }
        public bool RemoveAdminRole { get; set; }
        public bool AddUserRole { get; set; }
        public bool RemoveUserRole { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

public class AdminUpdateRolesDto
{
    public bool AddAdminRole { get; set; }
    public bool RemoveAdminRole { get; set; }
    public bool AddUserRole { get; set; }
    public bool RemoveUserRole { get; set; }
}

using System.ComponentModel.DataAnnotations;

public class AdminUpdatePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; }
    [Required]
    public string NewPassword { get; set; }
}
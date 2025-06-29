namespace Stockat.Core.DTOs.UserDTOs;

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}

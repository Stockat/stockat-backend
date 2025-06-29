namespace Stockat.Core.DTOs.UserDTOs;

public class UserUpdateDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? AboutMe { get; set; }
}

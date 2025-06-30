namespace Stockat.Core.DTOs.UserDTOs;

public class UserReadDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? AboutMe { get; set; }

    public string? ProfileImageUrl { get; set; }

    public bool IsApproved { get; set; }
    public bool IsDeleted { get; set; }

    public bool NeedsVerification => !IsApproved;

    public List<string> Roles { get; set; } = new();
}

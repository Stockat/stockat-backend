using System.ComponentModel.DataAnnotations;

namespace Stockat.Core.DTOs.UserDTOs;

public class UserForRegistrationDto
{
    public string FirstName { get; init; } // the init accessor is a special kind of setter introduced in C# 9.0. It allows properties to be set only during object initialization (i.e., when you create the object), but not modified afterward
    public string LastName { get; init; }
    [Required(ErrorMessage = "Username is required")]
    public string? UserName { get; init; }
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; init; }
    public string Email { get; init; }
    public string PhoneNumber { get; init; }
}

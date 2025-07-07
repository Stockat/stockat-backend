namespace Stockat.Core.DTOs.UserDTOs;
using Stockat.Core.DTOs.UserVerificationDTOs;

public class UserReadDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }

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
    
    // Admin-specific properties
    public PunishmentInfoDto? CurrentPunishment { get; set; }
    public List<PunishmentHistoryDto>? PunishmentHistory { get; set; }
    public UserStatisticsDto? Statistics { get; set; }

    public UserVerificationReadDto? UserVerification { get; set; }
}

public class PunishmentInfoDto
{
    public string Type { get; set; }
    public string Reason { get; set; }
    public DateTime? EndDate { get; set; }
}

public class PunishmentHistoryDto
{
    public string Type { get; set; }
    public string Reason { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class UserStatisticsDto
{
    public int TotalProducts { get; set; }
    public int TotalServices { get; set; }
    public int TotalAuctions { get; set; }
    public int TotalPunishments { get; set; }
    public int ActivePunishments { get; set; }
}

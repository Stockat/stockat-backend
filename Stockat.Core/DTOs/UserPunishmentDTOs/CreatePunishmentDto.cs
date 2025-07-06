using System.ComponentModel.DataAnnotations;
using Stockat.Core.Enums;

namespace Stockat.Core.DTOs.UserPunishmentDTOs;

public class CreatePunishmentDto
{
    [Required]
    public string UserId { get; set; }
    
    [Required]
    public PunishmentType Type { get; set; }
    
    [Required]
    [MinLength(10, ErrorMessage = "Reason must be at least 10 characters long.")]
    public string Reason { get; set; }
    
    public DateTime? EndDate { get; set; } // Required for TemporaryBan, null for PermanentBan
} 
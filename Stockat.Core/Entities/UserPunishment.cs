using System.ComponentModel.DataAnnotations;
using Stockat.Core.Enums;

namespace Stockat.Core.Entities;

public class UserPunishment
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [Required]
    public PunishmentType Type { get; set; }
    
    [Required]
    public string Reason { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; } // null = permanent
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; }
} 
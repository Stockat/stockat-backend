using Stockat.Core.Enums;

namespace Stockat.Core.DTOs.UserPunishmentDTOs;

public class PunishmentReadDto
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public PunishmentType Type { get; set; }
    public string Reason { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 
using Stockat.Core.Enums;

namespace Stockat.Core.DTOs.ServiceRequestUpdateDTOs;

public class ServiceRequestUpdateDto
{
    public int Id { get; set; }
    public decimal TotalOldPrice { get; set; }
    public decimal AdditionalPrice { get; set; }
    public int AdditionalQuantity { get; set; }
    public string? AdditionalTime { get; set; }
    public string? AdditionalNote { get; set; }
    public ApprovalStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

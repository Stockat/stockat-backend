using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.ServiceDTOs;

public class ServiceEditRequestDto
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string EditedName { get; set; }
    public string EditedDescription { get; set; }
    public int EditedMinQuantity { get; set; }
    public decimal EditedPricePerProduct { get; set; }
    public string EditedEstimatedTime { get; set; }
    public string EditedImageId { get; set; }
    public string EditedImageUrl { get; set; }
    public Stockat.Core.Enums.EditApprovalStatus ApprovalStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? AdminNote { get; set; }
}


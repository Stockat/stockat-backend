using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.ServiceDTOs;

public class ServiceEditRequestDto
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    
    // Current service values
    public string CurrentName { get; set; }
    public string CurrentDescription { get; set; }
    public int CurrentMinQuantity { get; set; }
    public decimal CurrentPricePerProduct { get; set; }
    public string CurrentEstimatedTime { get; set; }
    public string CurrentImageUrl { get; set; }
    
    // New values
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
    public bool IsReactivationRequest { get; set; }
}

public class CreateServiceEditRequestDto
{
    [Required]
    [MaxLength(100)]
    public string EditedName { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string EditedDescription { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int EditedMinQuantity { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal EditedPricePerProduct { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string EditedEstimatedTime { get; set; }
    
    [MaxLength(255)]
    public string EditedImageId { get; set; }
    
    [MaxLength(2083)]
    public string EditedImageUrl { get; set; }
}


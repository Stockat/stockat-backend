using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.Enums;

namespace Stockat.Core.Entities;

public class ServiceEditRequest
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [ForeignKey("ServiceId")]
    public Service Service { get; set; }

    [Required]
    [MaxLength(100)]
    public string EditedName { get; set; }

    [Required]
    [MaxLength(1000)]
    public string EditedDescription { get; set; }

    [Required]
    public int EditedMinQuantity { get; set; }

    [Required]
    public decimal EditedPricePerProduct { get; set; }

    [Required]
    [MaxLength(50)]
    public string EditedEstimatedTime { get; set; }

    [MaxLength(255)]
    public string EditedImageId { get; set; }

    [MaxLength(2083)]
    public string EditedImageUrl { get; set; }

    [Required]
    [MaxLength(20)]
    public EditApprovalStatus ApprovalStatus { get; set; } = EditApprovalStatus.Pending;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public string? AdminNote { get; set; }

    public bool IsReactivationRequest { get; set; } = false;
}

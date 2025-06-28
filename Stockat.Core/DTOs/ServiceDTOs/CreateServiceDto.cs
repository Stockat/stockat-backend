using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.ServiceDTOs;

public class CreateServiceDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; }

    [Range(1, int.MaxValue)]
    public int MinQuantity { get; set; } = 1;

    [Range(0.01, double.MaxValue)]
    public decimal PricePerProduct { get; set; }

    [Required]
    [MaxLength(50)]
    public string EstimatedTime { get; set; }

    [Required]
    [MaxLength(255)]
    public string ImageId { get; set; }

    [Required]
    [Url]
    [MaxLength(2083)]
    public string ImageUrl { get; set; }
}

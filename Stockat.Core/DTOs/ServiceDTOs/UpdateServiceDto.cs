using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.ServiceDTOs;

public class UpdateServiceDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int? MinQuantity { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? PricePerProduct { get; set; }

    [MaxLength(50)]
    public string? EstimatedTime { get; set; }

    [MaxLength(255)]
    public string? ImageId { get; set; }

    [Url]
    [MaxLength(2083)]
    public string? ImageUrl { get; set; }
}
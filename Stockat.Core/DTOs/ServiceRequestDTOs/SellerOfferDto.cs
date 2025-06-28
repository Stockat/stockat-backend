using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.ServiceRequestDTOs;

public class SellerOfferDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal PricePerProduct { get; set; }

    [Required]
    [MaxLength(100)]
    public string EstimatedTime { get; set; }
}


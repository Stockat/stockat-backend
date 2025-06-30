using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.ServiceRequestDTOs;

public class CreateServiceRequestDto
{
    [Required]
    public int ServiceId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string RequestDescription { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int RequestedQuantity { get; set; }

}


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.Enums;

namespace Stockat.Core.DTOs.ServiceRequestUpdateDTOs;

public class CreateServiceRequestUpdateDto
{
    public decimal AdditionalPrice { get; set; }
    public int AdditionalQuantity { get; set; }
    public string? AdditionalTime { get; set; }
    public string? AdditionalNote { get; set; }  // e.g., "Expedited delivery"
}

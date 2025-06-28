using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.ServiceRequestUpdateDTOs;

public class ServiceRequestUpdateApprovalDto
{
    [Required(ErrorMessage = "Service Request Update ID is required.")]
    public bool Approved { get; set; }
}

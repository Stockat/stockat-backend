using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.Enums;

namespace Stockat.Core.DTOs.ServiceRequestDTOs;

public class ApprovalStatusDto
{
    [Required(ErrorMessage = "Status is required.")]
    [EnumDataType(typeof(ApprovalStatus))]
    public ApprovalStatus Status {get; set;}
}

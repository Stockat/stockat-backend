using System.ComponentModel.DataAnnotations;
using Stockat.Core.Enums;

namespace Stockat.Core.DTOs.ServiceRequestDTOs;

public class ServiceStatusDto
{
    [Required(ErrorMessage = "Status is required.")]
    [EnumDataType(typeof(ServiceStatus))]
    public ServiceStatus Status { get; set; }
}

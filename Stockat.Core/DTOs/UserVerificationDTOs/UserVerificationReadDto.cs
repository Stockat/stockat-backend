using Stockat.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.UserVerificationDTOs;

public class UserVerificationReadDto
{
    public string UserId { get; set; }
    public string NationalId { get; set; }
    public string ImageURL { get; set; }
    public VerificationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

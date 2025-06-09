using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.UserVerificationDTOs;

public class UserVerificationCreateDto
{
    [RegularExpression(@"^[2-3]\d{13}$", ErrorMessage = "National ID must be 14 digits and start with 2 or 3.")]
    public string NationalId { get; set; }

    [Required]
    public IFormFile Image { get; set; }
}

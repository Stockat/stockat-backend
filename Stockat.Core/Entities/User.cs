using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    [RegularExpression(@"^[2-3]\d{13}$", ErrorMessage = "National ID must be 14 digits and start with 2 or 3.")]
    public string NationalId { get; set; }
    public bool Approved { get; set; } // to be reviewed --> defaults to false; will be set to true only after National ID is validated

    // Navigation Properties

    public virtual ICollection<Product> Products { get; set; }
}
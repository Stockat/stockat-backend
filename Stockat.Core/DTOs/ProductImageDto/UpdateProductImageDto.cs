using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.ProductImageDto;

public class UpdateProductImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

}

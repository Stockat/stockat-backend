using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.CategoryDtos;

public class CategoryDto
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool isDeleted { get; set; }

}

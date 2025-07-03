using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.TagsDtos;

public class UpdateTagDto
{
    public int TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
}

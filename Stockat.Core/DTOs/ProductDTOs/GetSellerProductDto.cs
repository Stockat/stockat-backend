using Stockat.Core.DTOs.FeatureDtos;
using Stockat.Core.DTOs.ProductImageDto;
using Stockat.Core.DTOs.TagsDtos;
using Stockat.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.ProductDTOs;

public class GetSellerProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductStatus ProductStatus { get; set; }

    public decimal Price { get; set; }
    public string SellerId { get; set; }
    public bool canBeRequested { get; set; }
    public List<string> Image { get; set; } = new();

}

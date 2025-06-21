using Stockat.Core.DTOs.FeatureDtos;
using Stockat.Core.DTOs.ProductImageDto;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.ProductDTOs;

public class AddProductDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ProductStatus ProductStatus { get; set; } = ProductStatus.Pending;

    public decimal Price { get; set; }
    public int MinQuantity { get; set; }

    public string SellerId { get; set; }

    public virtual ICollection<AddFeatureDto> Features { get; set; } = new List<AddFeatureDto>();
    public virtual ICollection<AddProductmageDto> Images { get; set; } = new List<AddProductmageDto>();

}

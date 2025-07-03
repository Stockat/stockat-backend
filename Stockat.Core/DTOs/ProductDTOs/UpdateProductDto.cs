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

public class UpdateProductDto

{

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProductStatus ProductStatus { get; set; } = ProductStatus.Pending;

    public decimal Price { get; set; }
    public int MinQuantity { get; set; }
    public int CategoryId { get; set; }

    public string SellerId { get; set; }
    public string Location { get; set; }

    //public string[] Images { get; set; }

    public virtual ICollection<UpdateFeatureDto> Features { get; set; } = new List<UpdateFeatureDto>();
    public virtual ICollection<UpdateProductImageDto> Images { get; set; } = new List<UpdateProductImageDto>();
    public virtual ICollection<UpdateTagDto> ProductTags { get; set; } = new List<UpdateTagDto>();

}

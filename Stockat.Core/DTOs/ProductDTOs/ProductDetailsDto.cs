using Stockat.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.ProductDTOs;

public class ProductDetailsDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ApprovalStatus ProductStatus { get; set; } = ApprovalStatus.Pending;

    public decimal Price { get; set; }

    public string SellerId { get; set; }
    public string SellerName { get; set; }
    public int MinQuantity { get; set; }


    //Array of images
    public virtual ICollection<string> ImagesArr { get; set; }
}

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

    public ProductStatus ProductStatus { get; set; } = ProductStatus.Pending;

    public decimal Price { get; set; }

    public string SellerId { get; set; }
    public string SellerName { get; set; }
    public string CategoryName { get; set; }
    public int MinQuantity { get; set; }

    public bool CanBeRequested { get; set; }


    //Array of images
    public virtual ICollection<string> ImagesArr { get; set; }
}

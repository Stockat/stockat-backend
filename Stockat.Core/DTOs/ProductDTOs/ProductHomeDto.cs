using Stockat.Core.Entities;
using Stockat.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.ProductDTOs;

public class ProductHomeDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ProductStatus ProductStatus { get; set; } = ProductStatus.Pending;

    public decimal Price { get; set; }

    public bool isDeleted { get; set; } = false;

    public int MinQuantity { get; set; }

    //ForeignKey
    public string SellerId { get; set; }

    //Array of images
    public ICollection<string> Images { get; set; } = new List<string>();
}

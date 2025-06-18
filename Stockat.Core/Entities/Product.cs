using Stockat.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Entities;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Product Name is Required")]
    [MinLength(5, ErrorMessage = "Product Name Length Must Be Greater than or equal 5 char")]
    [MaxLength(50, ErrorMessage = "Product Name Length Must Be less than or equal 50 char")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product Description is Required")]
    [MinLength(5, ErrorMessage = "Product Description Length Must Be Greater than or equal 5 char")]
    [MaxLength(250, ErrorMessage = "Product Description Length Must Be less than or equal 250 char")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price Is Required")]
    [Range(1, float.MaxValue, ErrorMessage = "Price must be between 1 :999999 ")]
    public decimal Price { get; set; }



    [Required(ErrorMessage = "Minimum Quantity Is Required")]
    [Range(1, int.MaxValue, ErrorMessage = "Minimum Quantity must be between 1 :999999 ")]
    public int MinQuantity { get; set; }

    public ApprovalStatus ProductStatus { get; set; } = ApprovalStatus.Pending;

    public bool isDeleted { get; set; } = false;

    //ForeignKey
    [Required(ErrorMessage = "Seller Id is Required")]
    public string SellerId { get; set; }



    // Navigation Properties
    public virtual ICollection<ProductImage> Images { get; set; }
    public virtual ICollection<Stock> Stocks { get; set; }
    public virtual ICollection<OrderProduct> OrderProducts { get; set; }
    public virtual User User { get; set; }
    public ICollection<Auction> Auctions { get; set; } = new List<Auction>(); //partial 1-M with Auction 
    public virtual ICollection<ProductTag> ProductTags { get; set; }
    public virtual ICollection<Feature> Features { get; set; }


}

public class ProductImage
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "ImageUrl is Required")]
    public string ImageUrl { get; set; } = string.Empty;

    //Foreign Key
    [Required(ErrorMessage = "Product Id is Required")]
    public int ProductId { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; }
}
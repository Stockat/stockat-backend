using Stockat.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Entities;

public class OrderProduct
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Quantity is Required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be between 1 : 214783647")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Price is Required")]
    [Range(1, int.MaxValue, ErrorMessage = "Price must be between 1 : 214783647")]
    public decimal Price { get; set; }

    [Required]
    public OrderType OrderType { get; set; } = OrderType.Order; // Default to Order, can be Request

    public OrderStatus Status { get; set; }
    public DateTime CraetedAt { get; set; } = DateTime.Now;

    // Payment Info
    public string PaymentId { get; set; }
    public string PaymentStatus { get; set; }

    // ForeinKey
    public int ProductId { get; set; }
    public int StockId { get; set; }
    public string SellerId { get; set; }
    public string BuyerId { get; set; }

    // Navigation Properties 

    public virtual Product Product { get; set; }
    public virtual Stock Stock { get; set; }
    public virtual User Seller { get; set; }
    public virtual User Buyer { get; set; }


    [Required(ErrorMessage = "Request Product Description is Required")]
    [MinLength(5, ErrorMessage = "Request Product Description Length Must Be Greater than or equal 5 char")]
    [MaxLength(250, ErrorMessage = "Request Product Description Length Must Be less than or equal 250 char")]
    public string? Description { get; set; } = string.Empty;

}


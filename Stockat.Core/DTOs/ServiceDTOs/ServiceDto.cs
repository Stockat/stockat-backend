using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.ServiceDTOs;

public class ServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int MinQuantity { get; set; }
    public decimal PricePerProduct { get; set; }
    public string EstimatedTime { get; set; }
    public string ImageId { get; set; }
    public string ImageUrl { get; set; }
    public string SellerId { get; set; }
    public string SellerName { get; set; }
}


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core.DTOs.ServiceRequestUpdateDTOs;

namespace Stockat.Core.DTOs.ServiceRequestDTOs;

public class ServiceRequestDto
{
    public int Id { get; set; }

    public int ServiceId { get; set; }
    public string ServiceTitle { get; set; }

    public string RequestDescription { get; set; }
    public int RequestedQuantity { get; set; }

    public string? ImageId { get; set; }
    public string? ImageUrl { get; set; }

    public string BuyerId { get; set; }
    public string BuyerName { get; set; }

    public decimal PricePerProduct { get; set; }
    public decimal TotalPrice { get; set; }

    public string? EstimatedTime { get; set; }

    public string SellerApprovalStatus { get; set; }
    public string BuyerApprovalStatus { get; set; }
    public string ServiceStatus { get; set; }
    public string PaymentStatus { get; set; }
    public string SellerId { get; set; }
    public string SellerName { get; set; }
    public int SellerOfferAttempts { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ServiceRequestUpdateDto> RequestUpdates { get; set; } = new();

    // SNAPSHOT FIELDS
    public string ServiceNameSnapshot { get; set; }
    public string? ServiceDescriptionSnapshot { get; set; }
    public int ServiceMinQuantitySnapshot { get; set; }
    public decimal ServicePricePerProductSnapshot { get; set; }
    public string? ServiceEstimatedTimeSnapshot { get; set; }
    public string? ServiceImageUrlSnapshot { get; set; }
}

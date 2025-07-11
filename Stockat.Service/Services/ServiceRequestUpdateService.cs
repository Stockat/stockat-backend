﻿using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ServiceRequestDTOs;
using Stockat.Core.DTOs.ServiceRequestUpdateDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;

namespace Stockat.Service.Services;

public class ServiceRequestUpdateService : IServiceRequestUpdateService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repo;
    private readonly IEmailService _emailService;

    public ServiceRequestUpdateService(
        ILoggerManager logger,
        IMapper mapper,
        IRepositoryManager repo,
        IEmailService emailService
        )
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
        _emailService = emailService;
    }

    public async Task<bool> CancelUpdateAsync(int updateId, string buyerId)
    {
        var update = await _repo.ServiceRequestUpdateRepo.FindAsync(
            u => u.Id == updateId && u.ServiceRequest.BuyerId == buyerId,
            ["ServiceRequest"]
        );

        if (update == null)
        {
            _logger.LogError($"Service request update {updateId} not found for buyer {buyerId}.");
            throw new NotFoundException("Service request update not found or you are not the owner.");
        }

        if (update.Status != ApprovalStatus.Pending)
        {
            _logger.LogError($"Service request update {updateId} is not in pending status.");
            throw new BadRequestException("Service request update is not in pending status.");
        }

        update.Status = ApprovalStatus.Cancelled;
        _repo.ServiceRequestUpdateRepo.Update(update);
        await _repo.CompleteAsync();

        _logger.LogInfo($"Service request update {updateId} cancelled successfully by buyer {buyerId}.");
        return true;
    }

    public async Task<ServiceRequestUpdateDto> CreateUpdateAsync(int requestId, string buyerId, CreateServiceRequestUpdateDto dto)
    {

        var prevPendingUpdates = await _repo.ServiceRequestUpdateRepo.FindAllAsync(
            u => u.ServiceRequestId == requestId && u.Status == ApprovalStatus.Pending
        );

        if (prevPendingUpdates.Any())
        {
            _logger.LogError($"There is already a pending update for service request {requestId}.");
            throw new BadRequestException("There is already a pending update for this service request.");
        }


        var request = await _repo.ServiceRequestRepo.FindAsync(
            r => r.Id == requestId && r.BuyerId == buyerId,
            ["Buyer", "Service", "Service.Seller"]
        );

        if (request == null)
        {
            _logger.LogError($"Service request {requestId} not found for buyer {buyerId}.");
            throw new NotFoundException("Service request not found.");
        }

        if(request.ServiceStatus != ServiceStatus.Pending)
        {
            _logger.LogError($"Service request {requestId} is not in pending status.");
            throw new BadRequestException("Service request is not in pending status.");
        }

        if (request.SellerApprovalStatus != ApprovalStatus.Approved ||
                    request.BuyerApprovalStatus != ApprovalStatus.Approved)
        {
            throw new BadRequestException("Both seller and buyer must approve the request before updates can be made.");
        }

        var update = new ServiceRequestUpdate
        {
            TotalOldPrice = request.TotalPrice,
            AdditionalPrice = dto.AdditionalPrice,
            AdditionalQuantity = dto.AdditionalQuantity,
            AdditionalTime = dto.AdditionalTime,
            Status = ApprovalStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ServiceRequestId = requestId,
            AdditionalNote = dto.AdditionalNote
        };

        await _repo.ServiceRequestUpdateRepo.AddAsync(update);
        await _repo.CompleteAsync();

        // Send email notification to seller
        await _emailService.SendEmailAsync(
            request.Service.Seller.Email,
            "New Service Request Update",
            $"A new update has been requested for your service request '{request.Service.Name}'. Please review it in your dashboard."
        );
        return _mapper.Map<ServiceRequestUpdateDto>(update);
    }

    public async Task<ServiceRequestDto> HandleSellerApprovalAsync(int updateId, string sellerId, bool approved)
    {
        var update = await _repo.ServiceRequestUpdateRepo.FindAsync(
            u => u.Id == updateId && u.ServiceRequest.Service.SellerId == sellerId,
            ["ServiceRequest", "ServiceRequest.Service", "ServiceRequest.Buyer"]
        );

        if (update == null)
        {
            _logger.LogError($"Update {updateId} not found for seller {sellerId}.");
            throw new NotFoundException("Update not found or you are not the seller.");
        }


        if (update.Status != ApprovalStatus.Pending)
        {
            _logger.LogError($"Update {updateId} is not in pending status.");
            throw new BadRequestException("Update is not in pending status.");
        }


        var request = update.ServiceRequest;

        if (approved)
        {
            request.RequestedQuantity += update.AdditionalQuantity;
            request.TotalPrice = update.TotalOldPrice + (update.AdditionalQuantity * request.PricePerProduct) + update.AdditionalPrice;

            update.Status = ApprovalStatus.Approved;

            if (!string.IsNullOrEmpty(update.AdditionalTime) && !string.IsNullOrEmpty(request.EstimatedTime))
            {
                try
                {
                    var currentTime = ParseTime(request.EstimatedTime);
                    var additionalTime = ParseTime(update.AdditionalTime);

                    var totalTime = currentTime + additionalTime;
                    request.EstimatedTime = FormatTimeSpan(totalTime);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to parse or sum estimated time: {ex.Message}");
                    throw new BadRequestException("Invalid estimated time format.");
                }
            }

            _repo.ServiceRequestRepo.Update(request);
        }
        else
        {
            _logger.LogInfo($"Update {updateId} rejected by seller {sellerId}.");
            update.Status = ApprovalStatus.Rejected;
        }

        _repo.ServiceRequestUpdateRepo.Update(update);
        await _repo.CompleteAsync();

        // Send email notification to buyer
        await _emailService.SendEmailAsync(
            request.Buyer.Email,
            "Service Request Update Status",
            $"Your service request update has been {(approved ? "approved" : "rejected")} by the seller. Please check your dashboard for details."
        );

        return _mapper.Map<ServiceRequestDto>(request);
    }
       
    public async Task<ServiceRequestUpdateDto> GetUpdateByIdAsync(int updateId, string userId)
    {
        var update = await _repo.ServiceRequestUpdateRepo.FindAsync(
            u => u.Id == updateId && (u.ServiceRequest.BuyerId == userId || u.ServiceRequest.Service.SellerId == userId),
            ["ServiceRequest", "ServiceRequest.Service"]
        );

        if (update == null)
        {
            _logger.LogError($"Service request update {updateId} not found.");
            throw new NotFoundException("Service request update not found.");
        }

        return _mapper.Map<ServiceRequestUpdateDto>(update);
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceRequestUpdateDto>>>> GetUpdatesByRequestIdAsync(int requestId, string userId, int page, int size)
    {
        int skip = (page - 1) * size;

        var updates = await _repo.ServiceRequestUpdateRepo.FindAllAsync(
            u => u.ServiceRequestId == requestId &&
                 (u.ServiceRequest.BuyerId == userId || u.ServiceRequest.Service.SellerId == userId),
            skip,
            size,
            includes: ["ServiceRequest", "ServiceRequest.Service"]
        );

        int totalCount = await _repo.ServiceRequestUpdateRepo.CountAsync(
            u => u.ServiceRequestId == requestId &&
                 (u.ServiceRequest.BuyerId == userId || u.ServiceRequest.Service.SellerId == userId)
        );

        var result = new PaginatedDto<IEnumerable<ServiceRequestUpdateDto>>
        {
            Page = page,
            Size = size,
            Count = totalCount,
            PaginatedData = _mapper.Map<IEnumerable<ServiceRequestUpdateDto>>(updates)
        };

        return new GenericResponseDto<PaginatedDto<IEnumerable<ServiceRequestUpdateDto>>>
        {
            Status = 200,
            Message = "Request updates retrieved successfully.",
            Data = result
        };
    }


    private TimeSpan ParseTime(string timeText)
    {
        timeText = timeText.ToLower().Trim();
        var match = System.Text.RegularExpressions.Regex.Match(timeText, @"(\d+)\s*(day|week|month)");
        if (!match.Success)
            throw new ArgumentException("Unsupported time format.");

        int value = int.Parse(match.Groups[1].Value);
        string unit = match.Groups[2].Value;

        switch (unit)
        {
            case "day":
                return TimeSpan.FromDays(value);
            case "week":
                return TimeSpan.FromDays(value * 7);
            case "month":
                return TimeSpan.FromDays(value * 30); // or 28/31 as you prefer
            default:
                throw new ArgumentException("Unsupported time unit.");
        }
    }

    private string FormatTimeSpan(TimeSpan span)
    {
        if (span.TotalDays >= 30 && span.TotalDays % 30 == 0)
        {
            return $"{(int)(span.TotalDays / 30)} month(s)";
        }
        else if (span.TotalDays >= 7 && span.TotalDays % 7 == 0)
        {
            return $"{(int)(span.TotalDays / 7)} week(s)";
        }
        else
        {
            return $"{(int)span.TotalDays} day(s)";
        }
    }


}

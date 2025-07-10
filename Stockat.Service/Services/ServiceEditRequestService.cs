using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Stockat.Core;
using Stockat.Core.DTOs;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;
using Stockat.Core.Shared;

namespace Stockat.Service.Services;

public class ServiceEditRequestService : IServiceEditRequestService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repo;
    private readonly IImageService _imageService;
    private readonly IEmailService _emailService;

    public ServiceEditRequestService(
        ILoggerManager logger,
        IMapper mapper,
        IRepositoryManager repo,
        IImageService imageService,
        IEmailService emailService)
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
        _imageService = imageService;
        _emailService = emailService;
    }
    public async Task ApproveEditRequestAsync(int requestId)
    {
        var request = await _repo.ServiceEditRequestRepo.FindAsync(r => r.Id == requestId && r.ApprovalStatus == EditApprovalStatus.Pending, includes: ["Service", "Service.Seller"]);
        if (request == null) throw new NotFoundException("Edit Request not found.");

        var service = request.Service;

        request.ReviewedAt = DateTime.UtcNow;
        request.ApprovalStatus = EditApprovalStatus.Approved;

        _mapper.Map(request, service); // Apply changes directly

        // For reactivation requests, also set service status to Approved
        if (request.IsReactivationRequest)
        {
            service.IsApproved = ApprovalStatus.Approved;
        }

        await _emailService.SendEmailAsync(
            service.Seller.Email,
            request.IsReactivationRequest ? "Service Reactivation Approved" : "Edit Request Approved",
            request.IsReactivationRequest ? "Your service has been reactivated and is now approved." : "Your service edit has been approved and applied."
        );

        await _repo.CompleteAsync();
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceEditRequestDto>>>> GetPendingRequestsAsync(int page = 1, int size = 10)
    {
        int skip = (page - 1) * size;

        // Get total count first
        var totalCount = await _repo.ServiceEditRequestRepo.CountAsync(r => r.ApprovalStatus == EditApprovalStatus.Pending);

        if (totalCount == 0)
        {
            _logger.LogInfo("No pending service edit requests found.");
            return new GenericResponseDto<PaginatedDto<IEnumerable<ServiceEditRequestDto>>>
            {
                Status = 200,
                Message = "No pending requests available",
                Data = new PaginatedDto<IEnumerable<ServiceEditRequestDto>>
                {
                    Page = page,
                    Size = size,
                    Count = 0,
                    PaginatedData = new List<ServiceEditRequestDto>()
                }
            };
        }

        // Get paginated data
        var requests = await _repo.ServiceEditRequestRepo.FindAllAsync(
           r => r.ApprovalStatus == EditApprovalStatus.Pending, 
           includes: ["Service"], skip: skip, take: size);

        var dtos = _mapper.Map<List<ServiceEditRequestDto>>(requests);
        
        // Populate current service values for comparison
        foreach (var dto in dtos)
        {
            var request = requests.FirstOrDefault(r => r.Id == dto.Id);
            if (request?.Service != null)
            {
                dto.CurrentName = request.Service.Name;
                dto.CurrentDescription = request.Service.Description;
                dto.CurrentMinQuantity = request.Service.MinQuantity;
                dto.CurrentPricePerProduct = request.Service.PricePerProduct;
                dto.CurrentEstimatedTime = request.Service.EstimatedTime;
                dto.CurrentImageUrl = request.Service.ImageUrl;
                
                // Debug logging for image URLs
                _logger.LogInfo($"Request {dto.Id}: CurrentImageUrl = '{dto.CurrentImageUrl}', EditedImageUrl = '{dto.EditedImageUrl}'");
            }
        }
        
        return new GenericResponseDto<PaginatedDto<IEnumerable<ServiceEditRequestDto>>>
        {
            Status = 200,
            Message = "Pending requests retrieved successfully",
            Data = new PaginatedDto<IEnumerable<ServiceEditRequestDto>>
            {
                Page = page,
                Size = size,
                Count = totalCount,
                PaginatedData = dtos
            }
        };
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceEditRequestDto>>>> GetApprovedRequestsAsync(int page = 1, int size = 10)
    {
        int skip = (page - 1) * size;

        // Get total count first
        var totalCount = await _repo.ServiceEditRequestRepo.CountAsync(r => r.ApprovalStatus == EditApprovalStatus.Approved);

        if (totalCount == 0)
        {
            _logger.LogInfo("No approved service edit requests found.");
            return new GenericResponseDto<PaginatedDto<IEnumerable<ServiceEditRequestDto>>>
            {
                Status = 200,
                Message = "No approved requests available",
                Data = new PaginatedDto<IEnumerable<ServiceEditRequestDto>>
                {
                    Page = page,
                    Size = size,
                    Count = 0,
                    PaginatedData = new List<ServiceEditRequestDto>()
                }
            };
        }

        // Get paginated data
        var requests = await _repo.ServiceEditRequestRepo.FindAllAsync(
           r => r.ApprovalStatus == EditApprovalStatus.Approved,
           includes: ["Service"], skip: skip, take: size);

        var dtos = _mapper.Map<List<ServiceEditRequestDto>>(requests);

        // Populate current service values for comparison
        foreach (var dto in dtos)
        {
            var request = requests.FirstOrDefault(r => r.Id == dto.Id);
            if (request?.Service != null)
            {
                dto.CurrentName = request.Service.Name;
                dto.CurrentDescription = request.Service.Description;
                dto.CurrentMinQuantity = request.Service.MinQuantity;
                dto.CurrentPricePerProduct = request.Service.PricePerProduct;
                dto.CurrentEstimatedTime = request.Service.EstimatedTime;
                dto.CurrentImageUrl = request.Service.ImageUrl;
                
                // Debug logging for image URLs
                _logger.LogInfo($"Request {dto.Id}: CurrentImageUrl = '{dto.CurrentImageUrl}', EditedImageUrl = '{dto.EditedImageUrl}'");
            }
        }

        return new GenericResponseDto<PaginatedDto<IEnumerable<ServiceEditRequestDto>>>
        {
            Status = 200,
            Message = "Approved requests retrieved successfully",
            Data = new PaginatedDto<IEnumerable<ServiceEditRequestDto>>
            {
                Page = page,
                Size = size,
                Count = totalCount,
                PaginatedData = dtos
            }
        };
    }

    public async Task<GenericResponseDto<PaginatedDto<IEnumerable<ServiceEditRequestDto>>>> GetRejectedRequestsAsync(int page = 1, int size = 10)
    {
        int skip = (page - 1) * size;

        // Get total count first
        var totalCount = await _repo.ServiceEditRequestRepo.CountAsync(r => r.ApprovalStatus == EditApprovalStatus.Rejected);

        if (totalCount == 0)
        {
            _logger.LogInfo("No rejected service edit requests found.");
            return new GenericResponseDto<PaginatedDto<IEnumerable<ServiceEditRequestDto>>>
            {
                Status = 200,
                Message = "No rejected requests available",
                Data = new PaginatedDto<IEnumerable<ServiceEditRequestDto>>
                {
                    Page = page,
                    Size = size,
                    Count = 0,
                    PaginatedData = new List<ServiceEditRequestDto>()
                }
            };
        }

        // Get paginated data
        var requests = await _repo.ServiceEditRequestRepo.FindAllAsync(
           r => r.ApprovalStatus == EditApprovalStatus.Rejected,
           includes: ["Service"], skip: skip, take: size);

        var dtos = _mapper.Map<List<ServiceEditRequestDto>>(requests);

        // Populate current service values for comparison
        foreach (var dto in dtos)
        {
            var request = requests.FirstOrDefault(r => r.Id == dto.Id);
            if (request?.Service != null)
            {
                dto.CurrentName = request.Service.Name;
                dto.CurrentDescription = request.Service.Description;
                dto.CurrentMinQuantity = request.Service.MinQuantity;
                dto.CurrentPricePerProduct = request.Service.PricePerProduct;
                dto.CurrentEstimatedTime = request.Service.EstimatedTime;
                dto.CurrentImageUrl = request.Service.ImageUrl;
                
                // Debug logging for image URLs
                _logger.LogInfo($"Request {dto.Id}: CurrentImageUrl = '{dto.CurrentImageUrl}', EditedImageUrl = '{dto.EditedImageUrl}'");
            }
        }

        return new GenericResponseDto<PaginatedDto<IEnumerable<ServiceEditRequestDto>>>
        {
            Status = 200,
            Message = "Rejected requests retrieved successfully",
            Data = new PaginatedDto<IEnumerable<ServiceEditRequestDto>>
            {
                Page = page,
                Size = size,
                Count = totalCount,
                PaginatedData = dtos
            }
        };
    }

    public async Task<GenericResponseDto<object>> GetRequestStatisticsAsync()
    {
        try
        {
            _logger.LogInfo("Starting to retrieve request statistics");
            
            // First, let's check if there are any records at all
            var totalRecords = await _repo.ServiceEditRequestRepo.CountAsync(s=>true);
            _logger.LogInfo($"Total records in ServiceEditRequest table: {totalRecords}");
            
            var pendingCount = await _repo.ServiceEditRequestRepo.CountAsync(r => r.ApprovalStatus == EditApprovalStatus.Pending);
            var approvedCount = await _repo.ServiceEditRequestRepo.CountAsync(r => r.ApprovalStatus == EditApprovalStatus.Approved);
            var rejectedCount = await _repo.ServiceEditRequestRepo.CountAsync(r => r.ApprovalStatus == EditApprovalStatus.Rejected);

            _logger.LogInfo($"Statistics - Pending: {pendingCount}, Approved: {approvedCount}, Rejected: {rejectedCount}");

            var statistics = new
            {
                Pending = pendingCount,
                Approved = approvedCount,
                Rejected = rejectedCount,
                Total = pendingCount + approvedCount + rejectedCount
            };

            _logger.LogInfo($"Returning statistics: {System.Text.Json.JsonSerializer.Serialize(statistics)}");

            return new GenericResponseDto<object>
            {
                Status = 200,
                Message = "Statistics retrieved successfully",
                Data = statistics
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving request statistics: {ex.Message}");
            throw;
        }
    }

    public async Task RejectEditRequestAsync(int requestId, string note)
    {
        var request = await _repo.ServiceEditRequestRepo.FindAsync(r => r.Id == requestId && r.ApprovalStatus == EditApprovalStatus.Pending, includes: ["Service", "Service.Seller"]);
        if (request == null) throw new NotFoundException("Edit request not found.");

        request.ApprovalStatus = EditApprovalStatus.Rejected;
        request.AdminNote = note;
        request.ReviewedAt = DateTime.UtcNow;

        // For reactivation requests, ensure service stays rejected
        if (request.IsReactivationRequest)
        {
            request.Service.IsApproved = ApprovalStatus.Rejected;
        }

        await _repo.CompleteAsync();

        if (!string.IsNullOrEmpty(request.Service.Seller?.Email))
        {
            await _emailService.SendEmailAsync(
                request.Service.Seller.Email, 
                request.IsReactivationRequest ? "Service Reactivation Rejected" : "Edit Request Rejected",
                request.IsReactivationRequest ? $"Your service reactivation has been rejected.\nAdmin Note: {note}" : $"Your service edit has been rejected.\nAdmin Note: {note}"
            );
        }
    }

    public async Task SubmitEditRequestAsync(int serviceId, string sellerId, CreateServiceEditRequestDto dto)
    {
        var service = await _repo.ServiceRepo.FindAsync(s => s.Id == serviceId && !s.IsDeleted) ?? throw new NotFoundException("Service not found.");

        if (service.SellerId != sellerId)
        {
            _logger.LogError($"Seller {sellerId} attempted to edit service {serviceId} they do not own.");
            throw new UnauthorizedAccessException("You do not own this service.");
        }

        // Prevent edit requests for rejected services
        if (service.IsApproved == ApprovalStatus.Rejected)
        {
            throw new BadRequestException("Cannot submit edit requests for rejected services. Please contact support to reactivate your service.");
        }

        var request = _mapper.Map<ServiceEditRequest>(dto);
        request.ServiceId = serviceId;

        await _repo.ServiceEditRequestRepo.AddAsync(request);
        await _repo.CompleteAsync();
    }

    public async Task ReactivateRejectedServiceAsync(int serviceId, string sellerId, CreateServiceEditRequestDto dto)
    {
        var service = await _repo.ServiceRepo.FindAsync(s => s.Id == serviceId && !s.IsDeleted) ?? throw new NotFoundException("Service not found.");

        if (service.SellerId != sellerId)
        {
            _logger.LogError($"Seller {sellerId} attempted to reactivate service {serviceId} they do not own.");
            throw new UnauthorizedAccessException("You do not own this service.");
        }

        // Only allow reactivation for rejected services
        if (service.IsApproved != ApprovalStatus.Rejected)
        {
            throw new BadRequestException("Only rejected services can be reactivated.");
        }

        var existing = await _repo.ServiceEditRequestRepo.AnyAsync(r => r.ServiceId == serviceId && r.ApprovalStatus == EditApprovalStatus.Pending);
        if (existing)
            throw new BadRequestException("You already have a pending reactivation request.");

        // Keep service status as rejected until reactivation is approved
        // service.IsApproved remains ApprovalStatus.Rejected

        var request = _mapper.Map<ServiceEditRequest>(dto);
        request.ServiceId = serviceId;
        request.IsReactivationRequest = true; // Mark as reactivation request

        await _repo.ServiceEditRequestRepo.AddAsync(request);
        await _repo.CompleteAsync();
    }
}

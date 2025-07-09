using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Stockat.Core;
using Stockat.Core.DTOs.ServiceDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;

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

        // Check for active service requests
        bool hasActiveRequests = await _repo.ServiceRequestRepo.AnyAsync(sr =>
            sr.ServiceId == service.Id &&
            sr.ServiceStatus != ServiceStatus.Delivered &&
            sr.ServiceStatus != ServiceStatus.Cancelled
        );

        request.ReviewedAt = DateTime.UtcNow;
        request.ApprovalStatus = EditApprovalStatus.Approved;

        if (hasActiveRequests)
        {
            request.IsDeferred = true;

            await _emailService.SendEmailAsync(
                service.Seller.Email,
                "Edit Request Approved (Deferred)",
                "Your service edit was approved but will be applied after current service requests are completed."
            );
        }
        else
        {
            _mapper.Map(request, service); // Apply changes directly

            await _emailService.SendEmailAsync(
                service.Seller.Email,
                "Edit Request Approved",
                "Your service edit has been approved and applied."
            );
        }

        service.Name = request.EditedName;
        service.Description = request.EditedDescription;
        service.MinQuantity = request.EditedMinQuantity;
        service.PricePerProduct = request.EditedPricePerProduct;
        service.EstimatedTime = request.EditedEstimatedTime;
        service.ImageId = request.EditedImageId;
        service.ImageUrl = request.EditedImageUrl;


        await _repo.CompleteAsync();
    }

    public async Task<List<ServiceEditRequestDto>> GetPendingRequestsAsync()
    {
        var requests = await _repo.ServiceEditRequestRepo.FindAllAsync(
           r => r.ApprovalStatus == EditApprovalStatus.Pending, 
           includes: ["Service"]);

        if (requests == null || !requests.Any())
        {
            _logger.LogInfo("No pending service edit requests found.");
            return new List<ServiceEditRequestDto>();
        }

        return _mapper.Map<List<ServiceEditRequestDto>>(requests);
    }

    public async Task RejectEditRequestAsync(int requestId, string note)
    {
        var request = await _repo.ServiceEditRequestRepo.FindAsync(r => r.Id == requestId && r.ApprovalStatus == EditApprovalStatus.Pending, includes: ["Service", "Service.Seller"]);
        if (request == null) throw new NotFoundException("Edit request not found.");

        request.ApprovalStatus = EditApprovalStatus.Rejected;
        request.AdminNote = note;
        request.ReviewedAt = DateTime.UtcNow;

        await _repo.CompleteAsync();

        await _emailService.SendEmailAsync(request.Service.Seller.Email, "Edit Request Rejected", "Your service edit has been rejected.");
    }

    public async Task SubmitEditRequestAsync(int serviceId, string sellerId, ServiceEditRequestDto dto)
    {
        var service = await _repo.ServiceRepo.FindAsync(s => s.Id == serviceId && !s.IsDeleted) ?? throw new NotFoundException("Service not found.");

        if (service.SellerId != sellerId)
        {
            _logger.LogError($"Seller {sellerId} attempted to edit service {serviceId} they do not own.");
            throw new UnauthorizedAccessException("You do not own this service.");
        }

        var existing = await _repo.ServiceEditRequestRepo.AnyAsync(r => r.ServiceId == serviceId && r.ApprovalStatus == EditApprovalStatus.Pending);
        if (existing)
            throw new BadRequestException("You already have a pending edit request.");

        var request = _mapper.Map<ServiceEditRequest>(dto);
        request.ServiceId = serviceId;


        await _repo.ServiceEditRequestRepo.AddAsync(request);
        await _repo.CompleteAsync();

    }

    public async Task ApplyDeferredEditsAsync(int serviceId)
    {
        // Find latest deferred approved edit
        var editRequest = await _repo.ServiceEditRequestRepo.FindAsync(
            r => r.ServiceId == serviceId && r.ApprovalStatus == EditApprovalStatus.Approved && r.IsDeferred
        );

        if (editRequest == null) return;

        var service = await _repo.ServiceRepo.FindAsync(s => s.Id == serviceId && !s.IsDeleted);
        if (service == null) return;

        _mapper.Map(editRequest, service);

        editRequest.IsDeferred = false;

        await _repo.CompleteAsync();

        await _emailService.SendEmailAsync(
            service.Seller.Email,
            "Deferred Edit Applied",
            "Your previously approved service edit has now been applied since all service requests are completed."
        );
    }

}

using System.Linq.Expressions;
using AutoMapper;
using Stockat.Core;
using Stockat.Core.DTOs.ServiceRequestDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.Core.Exceptions;
using Stockat.Core.IServices;

namespace Stockat.Service.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repo;

    public ServiceRequestService(
        ILoggerManager logger,
        IMapper mapper,
        IRepositoryManager repo
        )
    {
        _logger = logger;
        _mapper = mapper;
        _repo = repo;
    }

    public async Task<ServiceRequestDto> CreateAsync(CreateServiceRequestDto dto, string buyerId)
    {
        // Prevent duplicate pending request for the same service by the same buyer
        var existingPendingRequest = await _repo.ServiceRequestRepo.FindAsync(
            r => r.ServiceId == dto.ServiceId
              && r.BuyerId == buyerId
              && r.ServiceStatus == ServiceStatus.Pending
        );

        if (existingPendingRequest != null)
        {
            throw new BadRequestException("You already have a pending request for this service.");
        }

        // Fetch the service
        var service = await _repo.ServiceRepo.FindAsync(s => s.Id == dto.ServiceId);
        if (service == null)
        {
            _logger.LogError($"Service with ID {dto.ServiceId} not found.");
            throw new NotFoundException($"Service with ID {dto.ServiceId} not found.");
        }

        // Prevent buyer from creating request for their own service
        if (service.SellerId == buyerId)
        {
            _logger.LogError($"Cannot create service request for service {dto.ServiceId} by buyer {buyerId} as they are the seller.");
            throw new BadRequestException("Cannot create a service request for your own service.");
        }

        // Create the request
        var request = new ServiceRequest
        {
            ServiceId = dto.ServiceId,
            RequestDescription = dto.RequestDescription,
            RequestedQuantity = dto.RequestedQuantity,
            BuyerId = buyerId,
            SellerApprovalStatus = ApprovalStatus.Pending,
            BuyerApprovalStatus = ApprovalStatus.Pending,
            ServiceStatus = ServiceStatus.Pending,
            PricePerProduct = service.PricePerProduct, // Include the price from service
            TotalPrice = 0 // Total to be set after seller/buyer approval logic
        };

        await _repo.ServiceRequestRepo.AddAsync(request);
        await _repo.CompleteAsync();

        // Manually populate navigation properties (avoids extra query)
        request.Service = service;

        return _mapper.Map<ServiceRequestDto>(request);
    }


    public async Task<IEnumerable<ServiceRequestDto>> GetBuyerRequestsAsync(string buyerId)
    {
        var requests = await _repo.ServiceRequestRepo.FindAllAsync(r => r.BuyerId == buyerId, ["Buyer", "Service"]);
        if (requests == null || !requests.Any())
        {
            _logger.LogInfo($"No service requests found for buyer {buyerId}.");
            return Enumerable.Empty<ServiceRequestDto>();
        }

        _logger.LogInfo($"Retrieved {requests.Count()} service requests for buyer {buyerId}.");
        return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
    }

    public async Task<ServiceRequestDto> GetByIdAsync(int requestId, string userId, bool isSeller)
    {
        Expression<Func<ServiceRequest, bool>> predicate = isSeller
            ? req => req.Id == requestId && (req.Service.SellerId == userId || req.BuyerId == userId)
            : req => req.Id == requestId && req.BuyerId == userId;

        var request = await _repo.ServiceRequestRepo.FindAsync(predicate, ["Buyer", "Service"]);
        if (request == null)
        {
            var role = isSeller ? "Seller" : "Buyer";
            _logger.LogError($"Service request with ID {requestId} not found for {role} {userId}.");
            throw new NotFoundException($"Service request with ID {requestId} not found for {role} {userId}.");
        }

        _logger.LogInfo($"Service request with ID {requestId} retrieved successfully for user {userId}.");
        return _mapper.Map<ServiceRequestDto>(request);
    }

    public async Task<IEnumerable<ServiceRequestDto>> GetSellerRequestsAsync(string sellerId, int serviceId)
    {
        var requests = await _repo.ServiceRequestRepo.FindAllAsync(
            r => r.Service.SellerId == sellerId && r.ServiceId == serviceId,
            ["Buyer", "Service"]
        );

        if (requests == null || !requests.Any())
        {
            _logger.LogInfo($"No service requests found for seller {sellerId}.");
            return Enumerable.Empty<ServiceRequestDto>();
        }
        _logger.LogInfo($"Retrieved {requests.Count()} service requests for seller {sellerId}.");
        return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
    }

    public async Task<ServiceRequestDto> SetSellerOfferAsync(int requestId, string sellerId, SellerOfferDto dto)
    {
        var request = await _repo.ServiceRequestRepo.FindAsync(
            r => r.Id == requestId && r.Service.SellerId == sellerId,
            ["Buyer", "Service"]
        );

        if (request == null)
            throw new NotFoundException($"Service request {requestId} not found for this seller.");

        if (request.SellerApprovalStatus != ApprovalStatus.Pending)
            throw new BadRequestException("Seller already submitted an offer.");

        // Seller sets the initial offer
        request.PricePerProduct = dto.PricePerProduct;
        request.EstimatedTime = dto.EstimatedTime;
        request.TotalPrice = request.RequestedQuantity * dto.PricePerProduct;
        request.SellerApprovalStatus = ApprovalStatus.Approved;

        _repo.ServiceRequestRepo.Update(request);
        await _repo.CompleteAsync();

        return _mapper.Map<ServiceRequestDto>(request);
    }

    public async Task<ServiceRequestDto> UpdateBuyerStatusAsync(int requestId, string buyerId, ApprovalStatusDto statusDto)
    {
        var request = await _repo.ServiceRequestRepo.FindAsync(
            r => r.Id == requestId && r.BuyerId == buyerId,
            ["Buyer", "Service"]
        );

        if (request == null)
        {
            _logger.LogError($"Service request with ID {requestId} not found for buyer {buyerId}.");
            throw new NotFoundException($"Service request with ID {requestId} not found for buyer {buyerId}.");
        }

        if (request.SellerApprovalStatus != ApprovalStatus.Approved)
        {
            _logger.LogError($"Service request with ID {requestId} has not been approved by the seller.");
            throw new BadRequestException("You cannot update the status as the seller has not approved the request.");
        }

        if (request.BuyerApprovalStatus != ApprovalStatus.Pending)
        {
            _logger.LogError($"Service request with ID {requestId} has already been updated by the buyer.");
            throw new BadRequestException("You have already updated the status for this request.");
        }

        request.BuyerApprovalStatus = statusDto.Status;
      
        _repo.ServiceRequestRepo.Update(request);
        await _repo.CompleteAsync();

        _logger.LogInfo($"Buyer status for service request {requestId} updated to {statusDto.Status} by buyer {buyerId}.");
        return _mapper.Map<ServiceRequestDto>(request);
    }


    public Task<ServiceRequestDto> UpdatePaymentStatusAsync(int requestId, string paymentId, PaymentStatus status)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceRequestDto> UpdateServiceStatusAsync(int requestId, string sellerId, ServiceStatusDto dto)
    {
        var request = await _repo.ServiceRequestRepo.FindAsync(
            r => r.Id == requestId && r.Service.SellerId == sellerId,
            ["Service"]
        );

        if (request == null)
        {
            _logger.LogError($"Service request with ID {requestId} not found for seller {sellerId}.");
            throw new NotFoundException($"Service request with ID {requestId} not found for seller {sellerId}.");
        }

        if (request.SellerApprovalStatus != ApprovalStatus.Approved || request.BuyerApprovalStatus != ApprovalStatus.Approved)
        {
            _logger.LogError($"Service request with ID {requestId} has not been approved by the seller or the buyer.");
            throw new BadRequestException("You cannot update the service status as the request has not been approved by both parties.");
        }

        request.ServiceStatus = dto.Status;
        _repo.ServiceRequestRepo.Update(request);
        await _repo.CompleteAsync();

        _logger.LogInfo($"Service status for service request {requestId} updated to {dto} by seller {sellerId}.");
        return _mapper.Map<ServiceRequestDto>(request);
    }
}
